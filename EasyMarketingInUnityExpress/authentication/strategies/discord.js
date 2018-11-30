var passport = require('passport');
var fs = require('fs');
var mime = require('mime-types');
var path = require('path');
var WebSocket = require('ws');

var Facebook = require('passport-discord').Strategy;
var OAuth2 = require('oauth').OAuth2;

var clientKey = '474358511808282666';
var clientSecret = 'EyytWixfL12VKtUvwAlSFtpT0bqQSvIp';
var botToken = 'NDc0MzU4NTExODA4MjgyNjY2.DsI0-w.-zcMPGpPdgEtn-E7A0rkBm-eVkg';
var callbackURL = 'http://localhost:' + (process.env.PORT || '3000') + '/auth/Discord/callback';
var base = 'https://discordapp.com/api';

var oauth = new OAuth2(
    clientKey, 
    clientSecret,
    'https://discordapp.com/api',
    'oauth2/authorize',
    'oauth2/token',
    {'User-Agent': 'EasyMarketingInUnity (https://github.com/JoshuaKey/EasyMarketingInUnity, v0.5)' }
);
oauth.useAuthorizationHeaderforGET(true);
oauth.setAuthMethod('Bot');

var wss = null;

// Sets up the Strategy to be ready for authentication
var authenticate = function(){
    passport.use(new Facebook(
        {
        clientID: clientKey,
        clientSecret: clientSecret,
        callbackURL: callbackURL,
        passReqToCallback: true,
        }, 
        function(req, accessToken, refreshToken, params, profile, done){
            // Check if User is already added to the session?
            var user = req.user;
            if(!user){
                user = {}; 
            }
            
            console.log(accessToken);
            console.log(refreshToken);
            console.log(params);
            console.log(profile);
            
            user.discord = {};
            user.discord.id = profile.id;
            user.discord.bot = botToken;
            user.discord.token = accessToken;
            user.discord.tokenSecret = refreshToken;  
            
            user.discord.refreshTime = Date.now() + params.expires_in;
            user.discord.guild = params.guild.id;
            user.discord.webhooks = [];
            
            user.discord.likeEmo = oauth._encodeData('👍');
            user.discord.likeEmoName = 'thumbsup';        
            
            done(null, user);        
        }
    ));
}


var checkRefresh = function(user, done){
    done = done || (function() {});
    if(Date.now() < user.discord.refreshTime){
        done();
        return;
    }
    
    // May need to change 'client_id' -> user and secret -> password
    var code = user.discord.tokenSecret;
    var params = {
        grant_type: 'refresh_token',
    };
    var strategy = passport._strategy('discord');
    strategy.refresh(code, params, 
        function(err, accessToken, refreshToken, results){
            if(err){
                console.log(err);
                done();
                return;
            }
        
            user.discord.token = accessToken;
            if(refreshToken){
                 user.discord.tokenSecret = refreshToken;
            }
            user.discord.refreshTime = Date.now() + results.expires_in; 
        
            console.log(user);
            console.log(results);
        
            done();
        }
    );
}

var createHook = function(user, channel, done){  
    var url = base + '/channels/' + channel + '/webhooks';
    var obj = {
        "name": "EasyMarketingInUnityHook"
    }
    obj = JSON.stringify(obj);
    
    
    oauth.post(url,
        user.discord.bot,
        obj, 
        'application/json',
        function(err, results){
            if(err) { done(createDiscordError(err), null); return; }

            results = JSON.parse(results);
        
            var hook = {
                id: results.id,
                channel_id: results.channel_id,
                token: results.token,
            };
            user.discord.webhooks.push(hook);
            console.log('Webhooks: ', user.discord.webhooks);
        
            done(null, hook);
        }
    ); 
}

var getHook = function(user, channel, done){  
    var hook = null;
    if(user.discord.webhooks){
        hook = user.discord.webhooks.find(x => {
            return x.channel_id == channel;
        });
    }
    if(hook){
        done(null, hook);
        return;
    }    
    
    var url = base + '/channels/' + channel + '/webhooks';   
    
    oauth.get(url,
        user.discord.bot,
        function(err, results){
            if(err) { done(createDiscordError(err), null); return; }

            results = JSON.parse(results);
            console.log(results);
            if(results.length > 0){
                var hook = {
                    id: results[0].id,
                    channel_id: results[0].channel_id,
                    token: results[0].token,
                };
                user.discord.webhooks.push(hook);
                console.log('Webhooks: ', user.discord.webhooks);

                done(null, hook);
            } else {
                createHook(user, channel, done);
            }

        }
    );
}

// @User = User Object 
// @Guild = Guild ID
// @Done = should be a callback with (err, res)
var getChannels = function(user, done){
    var url = base + '/guilds/' + user.discord.guild + '/channels';

    oauth.get(url,
        user.discord.bot,
        function(err, results){
            if(err) { done(createDiscordError(err), null); return; }
            
            results = JSON.parse(results);
            done(null, results);
        }
    );
}

// @User = User Object 
// @Channel = Channel ID, can be DM, Guild Channel, Etc.
// @Done = should be a callback with (err, res)
var getMessages = function(user, channel, done){  
    var url = base + '/channels/' + channel + '/messages';
    
    oauth.get(url,
        user.discord.bot,
        function(err, results){
            if(err) { done(createDiscordError(err), null); return; }
            
            results = JSON.parse(results);
            done(null, results);
        }
    );
}

// @User = User Object containing Facebook data
// @Done = should be a callback with (err, res) (?)
var getLike = function(user, channel, message, done){
    var url = base + '/channels/' + channel + '/messages/' + message;
    
    oauth.get(url,
        user.discord.bot,
        function(err, results) {
            if(err) { done(createDiscordError(err), null); return; }

            results = JSON.parse(results);
            var hasLiked = false;
        
            if(results.reactions){
                for(var i = 0; i < results.reactions.length; i++){
                    var reaction = results.reactions[i];
                    if(reaction.me){
                        hasLiked = true;
                        break;
                    }
                }
            }
        
            results = Object.assign(results, { hasLiked: hasLiked });
            done(null, results);
        }
    );
}


// @User = User Object containing Facebook data
// @Channel = Channel ID
// @Message = Message to send
// @Done = should be a callback with (err, res) (?)
var postMessage = function(user, hook, message, done){        
    var url = base + '/webhooks/' + hook.id + '/' + hook.token;
    var obj = { 
        "content": message
    }
    obj = JSON.stringify(obj);

    oauth.post(url,
        user.discord.bot,
        obj, 
        'application/json',
        function(err, results){
            if(err) { done(createDiscordError(err), null); return; }

            //results = JSON.parse(results);
            done(null, results);
        }
    ); 
}

// @User = User Object containing Facebook data
// @Channel = Channel ID
// @File = URI to Media File
// @Done = should be a callback with (err, res) (?)
var postFile = function(user, hook, file, done){
    var readStream = fs.createReadStream(file);
    var data;

    readStream.on('end', upload);    
    readStream.on('error', (err) => {
        done(err, null);
    });
    readStream.on('data', (chunk) => {
        if(data){ data = Buffer.concat([data, chunk], data.length + chunk.length); }
        else { data = chunk; }
    });
    
    function upload(){
        var url = base + '/webhooks/' + hook.id + '/' + hook.token;
        var obj = { 
            payload_json: {
                "content": "Hello"
            },
            file: data
        }
        obj.payload_json = JSON.stringify(obj.payload_json);
        console.log(obj);

        oauth.post(url,
            user.discord.bot,
            obj, 
            'multipart/form-data',
            function(err, results){
                if(err) { done(createDiscordError(err), null); return; }

                //results = JSON.parse(results);
                done(null, results);
            }
        ); 
    };
}

// @User = User Object containing Facebook data
// @Channel = Channel ID
// @Message = Message to send
// @Emoji = Emoji ID to send
// @Done = should be a callback with (err, res) (?)
var postLike = function(user, channel, message, done){
    
    getLike(user, channel, message, function(err, results) {
        if(err){ done(err, null); return; } 
        
        var url = base + '/channels/' + channel + '/messages/' + message + '/reactions/' + user.discord.likeEmo + '/@me';
        console.log(url);
        
        if(results.hasLiked){
            oauth.delete(url,
                user.discord.bot,
                function(err, results){
                    if(err) { done(createDiscordError(err), null); return; }

                    done(null, results);
                }
            );
        } else {   
            oauth.put(url,
                user.discord.bot,
                '', 
                '',
                function(err, results){
                    if(err) { done(createDiscordError(err), null); return; }

                    done(null, results);
                }
            );
        }

    });
}

var postDM = function(user, recipient, done){
    var url = base + '/channels/' + recipient + '/messages';
    oauth.post(url,
        user.discord.bot,
        {recipient_id: recipient}, 
        'multipart/form-data',
        function(err, results){
            if(err) { done(createDiscordError(err), null); return; }

            results = JSON.parse(results);
            done(null, results);
        }
    ); 
}

var getGuildList = function(user, done){
    var url = base + '/guilds/' + user.discord.guild + '/members';
    oauth.get(url,
        user.discord.bot,
        function(err, results){
            if(err) { done(createDiscordError(err), null); return; }

            results = JSON.parse(results);
            done(null, results);
        }
    ); 
}

var connectToGateway = function(user, done){  
    var gatewayURL = '';
    var sessionStartObj = null;
    var heartbeatInteral = 0;
    
    function get(){
        var url = base + '/gateway/bot';

        oauth.get(url,
            user.discord.bot,
            function(err, results){
                console.log(err);
                console.log(results);
                if(err) { done(createDiscordError(err), null); return; }

                results = JSON.parse(results);
                
                gatewayURL = results.url;
                sessionStartObj = results.session_start_limit;
                
                connect();
            }
        );
    }   
    get();
    
    function connect(){
        wss = new WebSocket(gatewayURL);
        wss.heartbeatAck = true;
        wss.identify = false;
        wss.lastIdentify = 0;
        
        wss.on('open', function() {
            console.log('Open Websocket');
        });
        
        wss.on('ping', function(data){ // A Websocket should onyl ever send Pings and receive Pongs?
            console.log('Ping: ', data);
            wss.pong(null);
        });
        wss.on('pong', function(data){
            console.log('Pong: ', data);
//            wss.ping(null);
        });
        
        wss.on('close', function(code, str){
            console.log('Websocket Closing: (', code, ') ', str);
            wss = null;
        });
        wss.on('error', function(err){
            console.log('Websocket Error: ', err);
            wss = null;
        });

        wss.on('message', function(data) {
            //{"t":null,"s":null,"op":10,"d":{"heartbeat_interval":41250,"_trace":["gateway-prd-main-9hcd"]}}
            try{
                data = JSON.parse(data);
                console.log('Received: ', data);
                
                if(data.op == 10){ // Recieved OP Code 10 - Hello
                    heartbeatInteral = data.d.heartbeat_interval;
                    
                    sendHeartbeat(wss);
                } else if(data.op == 11){ // Recieved OP Code 11 - Heartbeat ACK
                    wss.heartbeatAck = true;
                } else if(data.op == 1){ // Recieved OP Code 1 - Heartbeat, Send ACK
                    wss.send({op: 11});
                } else if(data.t == 'READY'){ // Event 
                    wss.identify = true;
                    done();
                }
                
            } catch(err){
                console.log('Error: ', err);
                console.log('Received: ', data);
            }           
        });
        
        function sendHeartbeat(wss) {
            if(!wss.heartbeatAck){ // No heartbeat recieved, Close Connection
                
                console.log("No Heartbeat ACK recieved");
                
                wss.close(4009, 'No Heartbeat ACK recieved');
                return;
            }          
            
            wss.heartbeatAck = false;
            
            if(wss.readyState == wss.OPEN){
                var obj = {
                    "op": 1, 
                    "d": {}
                };
                wss.send(JSON.stringify(obj)); 
                console.log('Sent: ', JSON.stringify(obj));
                
                sendIdentify(wss);

                setTimeout(sendHeartbeat, heartbeatInteral, wss);
            }
        }
        function sendIdentify(wss){
            if(wss.identify || (Date.now() < wss.lastIdentify + 5000)){
                return;
            }
            
            wss.lastIdentify = Date.now();
            if(wss.readyState == wss.OPEN){
                var obj = {
                    "op": 2,
                    "d": {
                        "token": user.discord.bot,
                        "properties": {
                            "$os": "windows",
                            "$browser": "easymarketinginunity",
                            "$device": "easymarketinginunity",
                        }
                    }
                };
                console.log('Sent: ', JSON.stringify(obj));
                wss.send(JSON.stringify(obj));
            }
        }
    }
}

var disconnect = function(){
    if(wss){
         wss.close(1001, 'Disconnecting');
    }
}

// Takes in a Twitter response and returns a properly formated error.
var createDiscordError = function(res){
    // Example Error Response

    console.log(res);
    var data = JSON.parse(res.data);
    
    var error = {
        code: 0,
        message: ''
    };
    
    error.code = 4;
    if(data.message){
        error.message = data.code + ': ' + data.message;  
    } else {
        error.message = JSON.stringify(data);  
    }   
    
    return error;
}

// Return an object with service methods to make oAuth calls
exports.Service = {
    checkRefresh: checkRefresh,
    
    getChannels: getChannels,
    getMessages: getMessages,
    getLike: getLike,
    
    postMessage: postMessage,
    postFile: postFile,
    postLike: postLike,
    
    getHook: getHook
}

// Return a function to setup Authentcation
exports.Authentication = authenticate;