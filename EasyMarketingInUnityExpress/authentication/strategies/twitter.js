var passport = require('passport');
var fs = require('fs');
var mime = require('mime-types');
var path = require('path');

var Twitter = require('passport-twitter').Strategy;
var OAuth = require('oauth').OAuth;

var clientKey = '74Z7LvPzXiwM6PEYtrRe0NarU';
var clientSecret = 'JqKtnaruSDia50x4FbiNQ77S5ucXFFkTSEC5hRAP2HAmUtVaAM';
var callbackURL = 'http://localhost:' + (process.env.PORT || '3000') + '/auth/Twitter/callback';
var oauth = new OAuth(
    'https://api.twitter.com/oauth/request_token',
    'https://api.twitter.com/oauth/access_token',
    clientKey, 
    clientSecret, 
    '1.0A',
    null,
    'HMAC-SHA1'    
);

var MAX_FILE_SIZE_BYTES = 15 * 1024 * 1024;
var MAX_FILE_CHUNK_BYTES = 5 * 1024 * 1024;

// Sets up the Strategy to be ready for authentication
var authenticate = function(){
    passport.use(new Twitter(
        {
        consumerKey: clientKey,
        consumerSecret: clientSecret,
        callbackURL: callbackURL,
        passReqToCallback: true
        }, 
        function(req, token, tokenSecret, profile, done){
            // Check if User is already added to the session?
            var user = req.user;
            if(!user){
                user = {}; 
            }
//            console.log(profile);
            
            user.twitter = {};
            user.twitter.id = profile.id;
            user.twitter.token = token;
            user.twitter.tokenSecret = tokenSecret;
            user.twitter.username = profile.username;
            
            done(null, user);        
        }
    ));
}

// Returns the User's timeline
// Limit of 900 calls per 15 minutes
// @User = User Object containing Twitter ID, Token, and Secret
// @Done = should be a callback with (err, tweetObj) (?)
var getTimeline = function(user, done){
    oauth.get('https://api.twitter.com/1.1/statuses/user_timeline.json?user_id=' + user.twitter.id,
        user.twitter.token,
        user.twitter.tokenSecret,
        function(err, results, res){
            if(err) { done(createTwitterError(err), null); }
            else {
                results = JSON.parse(results);
                done(null, results);
            }
        }
    );
}

// Returns the specific Tweet in detail
// Limit of 900 calls per 15 minutes
// @User = User Object containing Twitter ID, Token, and Secret
// @Tweet = Tweet ID
// @Done = should be a callback with (err, tweetObj) (?)
var getTweet = function(user, tweet, done){
    var url = 'https://api.twitter.com/1.1/statuses/show.json?id=' + tweet + '&include_ext_alt_text=true';
    oauth.get(url,
        user.twitter.token,
        user.twitter.tokenSecret,
        function(err, results, res){
            if(err) { 
                done(createTwitterError(err), null); 
                return;
            }
            
            results = JSON.parse(results);
            done(null, results);
        }
    );
}

// Returns an array of tweets that are replies to the corresponding tweet
// Limit of 900 calls per 15 minutes
// @User = User Object containing Twitter ID, Token, and Secret
// @Tweet = Corresponding Tweet ID
// @Done = should be a callback with (err, tweetObj) (?)
var getReplies = function(user, tweet, done){
    
    var querySearch = oauth._encodeData('to:'+ user.twitter.username);
    var queryID = oauth._encodeData(tweet);
    var query = '?q=' + querySearch + '&since_id=' + queryID;
    oauth.get('https://api.twitter.com/1.1/search/tweets.json' + query,   
            user.twitter.token,
            user.twitter.tokenSecret,
            function(err, results, res) {
                if(err) { done(createTwitterError(err), null); return; }

                results = JSON.parse(results);
                console.log(results);

                var statuses = results.statuses;
                var replies = [];

                for(var i = 0; i < statuses.length; i++){
                    var status = statuses[i];
                    if(status.in_reply_to_status_id_str == tweet){
                        replies.push(status);
                    }
                }

                done(null, replies);
            }
    );
}

// Toggles the Tweet favorite.
// @User = User Object containing Twitter ID, Token, and Secret
// @Tweet = Tweet ID to Like or Unlike
// @Done = should be a callback with (err, tweetObj) (?)
var postLike = function(user, tweet, done) {    
    var query = '?id=' + tweet;   
    (function get(){
        oauth.get('https://api.twitter.com/1.1/statuses/show.json' + query,
            user.twitter.token,
            user.twitter.tokenSecret,
            function(err, results, res) {
                if(err) { done(createTwitterError(err), null); return; }

                results = JSON.parse(results);

                var fav = results.favorited; 
                toggleLike(fav);
            }
        );
    })();
    
    function toggleLike(liked){
        var path;
        if(liked){
            path = 'https://api.twitter.com/1.1/favorites/destroy.json';
        } else {
            path = 'https://api.twitter.com/1.1/favorites/create.json';
        }    
        
        oauth.post(path + query,
            user.twitter.token,
            user.twitter.tokenSecret,
            '',
            '',
            function(err, results, res) {
                if(err) { done(createTwitterError(err), null); return; }

                results = JSON.parse(results);

                done(null, results);
            }
        );
    }

}

// Sends an oAuth call to post a tweet. - DOES NOT WORK, Because oAuth Sig needs to be changed
// Limit of 300 per 3 hours
// @User = User Object containing Twitter ID, Token, and Secret
// @Status = Text to be displayed in the tweet
// @Media = URI to photo, gif, or video
// @Done = should be a callback with (err, tweetObj) (?)
var postMediaChunked = function(user, status, media, multiple, done){   
    function post(url, body, type, done){
        if (typeof body == 'function') {
            done = body;
            body = '';
        }
        if(typeof type == 'function'){
            done = type;
            type = 'application/x-www-form-urlencoded';
        }
        if(!body){
            body = '';
        }
        if(!type){
            type = 'application/x-www-form-urlencoded';
        }
        
        oauth.post(url,
            user.twitter.token,
            user.twitter.tokenSecret,
            body,
            type,
            done
        );
    }
    
    var url = 'https://upload.twitter.com/1.1/media/upload.json';
    var media_id;
    var stats;

    // Post INIT 
    fs.stat(media, function(err, _stats) {
        if(err) { done(err, null); return; }
        
        stats = _stats;
        
        var total_bytes = stats.size;
        var media_type = mime.lookup(media);

//        var query = '?command=INIT&total_bytes=' + total_bytes + '&media_type=' + media_type;
        var body = {command: 'INIT', total_bytes: total_bytes, media_type: media_type};
        console.log("Init Media");
        console.log(body);

        post(url, body, Append);
    });

    function Append(err, results){
        if(err) { done(err, null); return; }
        
        results = JSON.parse(results);
        console.log(results);
        
        console.log("Appending Data");
        
        media_id = results.media_id_string; // 64 bit number / string
        
        var segment_index = 0; // 0 - 100
//        var initQuery = '?command=APPEND&media_id=' + media_id;      
        var initBody = {command: 'APPEND', media_id: media_id};      
        var readStream = fs.createReadStream(media, { highWatermark: MAX_FILE_CHUNK_BYTES });
        var fsEnded = false;
        var uploading = false;
        
        // On End, Redirect to Finalize Method
        readStream.on('end', function() {
            fsEnded = true;
            if(!uploading){
                Finalize();
            }
        });
        
        // On Error, Return Error
        readStream.on('error', (err) => {
            console.log("Error in Read Stream");
            done(err, null);
        });
        
        // On Readable, try to read data and post to Twitter
        readStream.on('data', PostData.bind(readStream));
        
        function PostData(chunk){
            this.pause();
            uploading = true;
            
            var body = {command: 'APPEND', media_data: chunk.toString('base64'), media_id: media_id, segment_index: segment_index++};
//            var body = {segment_index: segment_index++, media_data: chunk.toString('base64')};
//            body = Object.assign(body, initBody);
            console.log(body);
            console.log(chunk.length);

            post(url, body, 'multipart/form-data', (err, results) => {
                uploading = false;
                
                console.log("Results of Data");
                console.log(err);
                console.log(results);                
                console.log("--------\n");
                
                if(err) { 
                    readStream.destroy(err);
                } else {
                    if(fsEnded) {
                        Finalize();
                    } else {
                        readStream.resume();
                    }
                }                 
            });
        }
        
    }

    function Finalize(){
//        var query = '?command=FINALIZE&media_id=' + media_id;
        var body = {command: 'FINALIZE', media_id: media_id};
        
        console.log("Finalize");
        console.log(body);
        
        post(url, body, Status);
    }
    
    function Status(err, tweet){
        if(err) { done(err, null); return; }
        tweet = JSON.parse(tweet);
          
        console.log("Checking Status");
        console.log(tweet);              
        
        if(tweet.processing_info){
            if(tweet.process_info.error) { 
                done(tweet.process_info.error, null); 
            } else if(tweet.process_info.state === "suceeded"){
                Tweet(err, tweet);
            } else {
                var check_after_secs = tweet.processing_info.check_after_secs;
                setTimeout(getStatus, check_after_secs*1000, Status);/// Example was 5, Thats a long time...
            }
        } else {
            Tweet(err, tweet);
        }
        
        function getStatus(callback) {
            var query = '?command=STATUS&media_id=' + media_id;
            oauth.get(url + query,
                user.twitter.token,
                user.twitter.tokenSecret,
                callback
            );         
        }
    }     

    function Tweet(err, tweet){
        if(err) { done(err, null); return; }

        tweet = JSON.parse(tweet);
        
        console.log(tweet);
        console.log("Tweeting");
        
        var leftover = null;
        if(multiple){
            leftover = status.substring(280);
            status = status.substring(0, 280);
        }  
       
//        var query = '?status=' + status + '&media_ids=' + media_id;
        oauth.post('https://api.twitter.com/1.1/statuses/update.json',
            user.twitter.token,
            user.twitter.tokenSecret,
            { status: status, media_ids : media_id},
            'application/json',
            function(err, results, res){
                if(err) { done(err, null); }
                else {
                    results = JSON.parse(results);
                    if(leftover && leftover !== ''){
                        postReplyChain(user, leftover, results.id_str, done)
                    } else {
                        done(null, results);
                    } 
                }
            }
        );
    }
 }

// Sends an oAuth call to post a tweet.
// Limit of 300 per 3 hours
// @User = User Object containing Twitter ID, Token, and Secret
// @Status = Text to be displayed in the tweet
// @Media = URI to photo, gif, or video
// @Multiple = Whether this should be a single tweet, or should create a reply chain
// @Done = should be a callback with (err, tweetObj) (?)
var postMediaSingle = function(user, status, media, multiple, done){  
    if(typeof status != "string"){
        status = "";
    }
    var readStream = fs.createReadStream(media);
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
        data = data.toString('base64');
        
        var writeStream = fs.createWriteStream('data2.txt');
        writeStream.write(data, () => {
            console.log('Finished');
        });
        
        oauth.post('https://upload.twitter.com/1.1/media/upload.json',
            user.twitter.token,
            user.twitter.tokenSecret,
            { media: data },
            "",
            tweet
        );
        //application/octet-stream
    };

    function tweet(err, results){
        if(err) { done(createTwitterError(err), null); return; }
        
        results = JSON.parse(results);
        var media_id = results.media_id_string;
        
        var leftover = null;
        if(multiple){
            leftover = status.substring(280);
            status = status.substring(0, 280);
        }  
        
        oauth.post('https://api.twitter.com/1.1/statuses/update.json',
            user.twitter.token,
            user.twitter.tokenSecret,
            { status: status, media_ids : media_id},
            'application/json',
            function(err, results, res){
                if(err) { done(createTwitterError(err), null); }
                else {
                    results = JSON.parse(results);
                    if(leftover && leftover !== ''){
                        postReplyChain(user, leftover, results.id_str, done)
                    } else {
                        done(null, results);
                    } 
                }
            }
        );
    }
}

// Sends an oAuth call to post a tweet.
// Limit of 300 per 3 hours
// @User = User Object containing Twitter ID, Token, and Secret
// @Status = Text to be displayed in the tweet
// @Multiple = Whether this should be a single tweet, or should create a reply chain
// @Done = should be a callback with (err, tweetObj) 
var postTweet = function(user, status, multiple, done){
    var leftover = null;
    if(multiple){
        leftover = status.substring(280);
        status = status.substring(0, 280);
    }  
    
    oauth.post('https://api.twitter.com/1.1/statuses/update.json',
        user.twitter.token,
        user.twitter.tokenSecret,
        { status: status },
        'application/json',
        function(err, results, res){
            if(err) { done(createTwitterError(err), null); }
            else {
                results = JSON.parse(results);
                if(leftover && leftover !== ''){
                    postReplyChain(user, leftover, results.id_str, done)
                } else {
                    done(null, results);
                } 
            }
        }
    );
}

// Sends an oAuth call to post a tweet.
// Limit of 300 per 3 hours
// @User = User Object containing Twitter ID, Token, and Secret
// @Status = Text to be displayed in the tweet
// @ReplyTo = Tweet ID to reply to. NOTE, the tweet must include @username of the original author
// @Multiple = Whether this should be a single tweet, or should create a reply chain
// @Done = should be a callback with (err, tweetObj) 
var postReply = function(user, status, replyTo, multiple, done){   
    getTweet(user, replyTo, function(err, res){
        if(err){ done(err, null); }

        reply(res.user.screen_name);
    });
    
    function reply(username){
        var leftover = null;
        if(multiple){
            var username = '@' + username + ' ';
            var length = 280 - username.length;
            
            leftover = status.substring(length);
            status = status.substring(0, length);
        }  
        
        
        oauth.post('https://api.twitter.com/1.1/statuses/update.json',
            user.twitter.token,
            user.twitter.tokenSecret,
            { status: status, in_reply_to_status_id: replyTo },
            '',
            function(err, results, res){
                if(err) { done(createTwitterError(err), null); }
                else {
                    results = JSON.parse(results);
                    
                    if(leftover && leftover !== ''){
                        postReplyChain(user, leftover, results.id_str, done)
                    } else {
                        done(null, results);
                    } 
                }
            }
        );
    }

}

// Creates a reply chain with the status given. 
// Note: ReplyTo should be a Tweet ID to the current user's tweets
// @User = User Object containing Twitter ID, Token, and Secret
// @Status = Text to be displayed in the tweet
// @ReplyTo = Tweet ID to reply to. 
// @Done = should be a callback with (err, tweetObj) 
var postReplyChain = function(user, status, replyTo, done){
    var leftover = null;
    
    var username = '@' + user.twitter.username + ' ';
    var length = 280 - username.length;
    leftover = status.substring(length);
    status = username + status.substring(0, length);

    oauth.post('https://api.twitter.com/1.1/statuses/update.json?',
        user.twitter.token,
        user.twitter.tokenSecret,
        { status: status, in_reply_to_status_id: replyTo },
        '',
        function(err, results, res){
            if(err) { done(createTwitterError(err), null); }
            else {
                results = JSON.parse(results);
                
                if(leftover && leftover != ''){
                    postReplyChain(user, leftover, results.id_str, done);
                } else {
                    done(null, results);
                }
            }
        }
    );
}

// Takes in a Twitter response and returns a properly formated error.
var createTwitterError = function(twitterRes){
    // Example Error Response
//    { statusCode: 404,
//  data:
//   '{"errors":[{"code":8,"message":"No data available for specified ID."}]}' }
    
    // OR
    
    // '{"request":"\\/1.1\\/media\\/upload.json","error":"media type unrecognized."}'
    console.log(twitterRes);
    var data = JSON.parse(twitterRes.data);
    
    var error = {
        code: 0,
        message: ''
    };
    
    error.code = 4;
    
    if(data.errors) {
        error.message = data.errors[0].code + ': ' + data.errors[0].message;
        for(var i = 1; i < data.errors.length; i++){
            error.message += '\n' + data.errors[i].code + ': ' + data.errors[i].message;
        } 
    }else {
        error.message = data.error;
    }   
    
    return error;
}

// Return an object with service methods to make oAuth calls
exports.Service = {
    getReplies: getReplies,
    getTweet: getTweet,
    getTimeline: getTimeline,
    postTweet: postTweet,
    postMedia: postMediaSingle,
    postReply: postReply,
    postLike: postLike
}

// Return a function to setup Authentcation
exports.Authentication = authenticate;