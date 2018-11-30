var passport = require('passport');

var path = require('path');
var fs = require('fs');
var mime = require('mime-types');

var Slack = require('passport-slack').Strategy;
var OAuth2 = require('oauth').OAuth2;

var SlackUpload = require('node-slack-upload');

var clientKey = '226752195127.478404776710';
var clientSecret = '2ed21c5dfb83644feea88d8738e5d526';
var callbackURL = 'http://localhost:' + (process.env.PORT || '3000') + '/auth/Slack/callback';

var oauth = new OAuth2(
    clientKey, 
    clientSecret,
    'https://slack.com',
    null,
    '/api/oauth.access'
);
oauth.useAuthorizationHeaderforGET(true);

var base = 'https://slack.com/api/';

// Sets up the Strategy to be ready for authentication
var authenticate = function(){
    passport.use(new Slack(
        {
        clientID: clientKey,
        clientSecret: clientSecret,
        callbackURL: callbackURL,
        //scope: ['identity.basic', 'identity.team', 'channels:history', 'channels:read', 'chat:write:user', 'stars:write'],
        passReqToCallback: true,
        }, 
        function(req, accessToken, refreshToken, profile, done){
            // Check if User is already added to the session?
            var user = req.user;
            if(!user){
                user = {}; 
            }
            
            console.log(accessToken);
            console.log(refreshToken);
            console.log(profile);
            
            user.slack = {};
            user.slack.id = profile.id;
            user.slack.token = accessToken;
            user.slack.tokenSecret = refreshToken;
            
            user.slack.teamID = profile.team.id;
            
            done(null, user);        
        }
    ));
}

var getChannels = function(user,done){
    var url = 'https://slack.com/api/users.conversations?exclude_archived=true';

    oauth.get(url,
        user.slack.token,
        function(err, results) {
            if(err) { done(createSlackError(err), null); return; }

            results = JSON.parse(results);
            done(null, results);
        }
    );
}

var getMessages = function(user, channel, done){
    var endpoint = channel.substring(0, 1) == 'G' ? 'groups.history' : 'channels.history';
    var url = base + endpoint + '?channel=' + channel;

    oauth.get(url,
        user.slack.token,
        function(err, results) {
            if(err) { done(createSlackError(err), null); return; }

            results = JSON.parse(results);
            done(null, results);
        }
    );
}

var postMessage = function(user, channel, message, done){
    var url = 'https://slack.com/api/chat.postMessage'
    
    var obj = {
        as_user: true,
        channel: channel,
        text: message,    
    };

    oauth.post(url,
        user.slack.token,
        obj,
        '',
        function(err, results) {
            if(err) { done(createSlackError(err), null); return; }

            results = JSON.parse(results);
            done(null, results);
        }
    );
}

var postFile = function(user, channel, file, message, done){
    if(typeof messageID === 'function'){
        done = messageID;
        messageID = null;
    }
    
    var obj = {
        file: fs.createReadStream(file),
        filetype: path.extname(file),
        filename: path.basename(file),
        channels: channel
    }
    if(message){
        obj.initial_comment = message
    }
    
    var slack = new SlackUpload(user.slack.token);
    
    slack.uploadFile(obj, function(err, results) {
        if(err) { done(createSlackError(err), null); return; }

        console.log(err);
        console.log(results);
//        results = JSON.parse(results);
        done(null, results);
    });
    

}

var postLike = function(user, channel, timestamp, done){
    
    function get(){
        var endpoint = channel.substring(0, 1) == 'G' ? 'groups.history' : 'channels.history';
        var url = base + endpoint + '?channel=' + channel + '&inclusive=true&count=1&latest=' + timestamp;
        
        console.log(url);
        
        oauth.get(url,
            user.slack.token,
            function(err, results) {
                if(err) { done(createSlackError(err), null); return; }

                results = JSON.parse(results);
                console.log(results);
                
                if(results.messages && results.messages.length > 0){
                    post(results.messages[0].is_starred);
                }
            }
        );
    }
    
    function post(hasLiked){
        var url;
        if(hasLiked){
            url = 'https://slack.com/api/stars.remove';
        } else {
            url = 'https://slack.com/api/stars.add'
        }
        
        var obj = {
            channel: channel,
            timestamp: timestamp
        };
        
        oauth.post(url,
            user.slack.token,
            obj,
            '',
            function(err, results) {
                if(err) { done(createSlackError(err), null); return; }

                results = JSON.parse(results);
                done(null, results);
            }
        );
    }
    
    get();
}

// Takes in a Twitter response and returns a properly formated error.
var createSlackError = function(err){
    // Example Error Response
    
    console.log(err);
    var data = JSON.parse(err.data);
    
    var error = {
        code: 0,
        message: ''
    };
    
    error.code = 4;
    

    error.message = '' + JSON.stringify(data);
       
    
    return error;
}

// Return an object with service methods to make oAuth calls
exports.Service = {
    getChannels: getChannels,
    getMessages: getMessages,
    
    postMessage: postMessage,
    postFile: postFile,
    postLike: postLike,
}

// Return a function to setup Authentcation
exports.Authentication = authenticate;