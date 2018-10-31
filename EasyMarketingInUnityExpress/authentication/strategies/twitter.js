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

// Sends an oAuth call to post a tweet. - DOES NOT WORK, Because oAuth Sig needs to be changed
// Limit of 300 per 3 hours
// @User = User Object containing Twitter ID, Token, and Secret
// @Status = Text to be displayed in the tweet
// @Media = URI to photo, gif, or video
// @Done = should be a callback with (err, tweetObj) (?)
var postMediaChunked = function(user, status, media, done){   
    function post(url, done){
        oauth.post(url,
            user.twitter.token,
            user.twitter.tokenSecret,
            '',
            'application/x-www-form-urlencoded',
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

        var query = '?command=INIT&total_bytes=' + total_bytes + '&media_type=' + media_type;

        post(url + query, Append);
    });

    function Append(err, tweet){
        if(err) { done(err, null); return; }
        
        tweet = JSON.parse(tweet);
        media_id = tweet.media_id_string; // 64 bit number / string
        
        var segment_index = 0; // 0 - 100
        var initQuery = '?command=APPEND&media_id=' + media_id;      
        var readStream = fs.createReadStream();
        
        // On End, Redirect to Finalize Method
        readStream.on('end', Finalize);
        
        // On Error, Return Error
        readStream.on('error', (err) => {
            done(err, null);
        });
        
        // On Readable, try to read data and post to Twitter
        readStream.on('readable', PostData.bind(this));
        
        function PostData(){
            var data;
            while(data = this.read(4096)) { // Find Max Byte Size for URL
                var query = initQuery + '&segment_index=' + (segment_index++) + '&media_data=' + data.toString('base64');
                
                post(url + query, (err, tweet) => {
                    console.log(tweet);
                    if(err) { 
                       readStream.destroy(err);
                    }
                });
            } 
        }
        
    }

    function Finalize(){
        var query = '?command=FINALIZE&media_id=' + media_id;
        post(url + query, Status);
    }
    
    function Status(err, tweet){
        if(err) { done(err, null); return; }
        tweet = JSON.parse(tweet);
        
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
       
        var query = '?status=' + status + '&media_ids=' + media_id;
        oauth.post('https://api.twitter.com/1.1/statuses/update.json' + query,
            user.twitter.token,
            user.twitter.tokenSecret,
            '',
            'application/json',
            function(err, results, res){
                if(err) { done(err, null); }
                else {
                    results = JSON.parse(results);
                    done(null, results);
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
        { status: status},
        'application/json',
        function(err, results, res){
            if(err) { done(createTwitterError(err), null); }
            else {
                results = JSON.parse(results);
                console.log('First Tweet');
                if(leftover && leftover !== ''){
                    postReplyChain(user, leftover, results.id_, done)
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
    var leftover = null;
    if(multiple){
        leftover = status.substring(280);
        status = status.substring(0, 280);
    } 
    var query = 'status=' + status + '&in_reply_to_status_id=' + replyTo;
    oauth.post('https://api.twitter.com/1.1/statuses/update.json?status='+status,
        user.twitter.token,
        user.twitter.tokenSecret,
        '',
        'application/json',
        function(err, results, res){
            if(err) { done(createTwitterError(err), null); }
            else {
                results = JSON.parse(results);
                if(leftover){
                    postReply(user, leftover, results.id_str, multiple, done)
                } else {
                    done(null, results);
                }
            }
        }
    );
}

// Creates a reply chain with the status given. 
// Note: ReplyTo should be a Tweet ID to the current user's tweets
// @User = User Object containing Twitter ID, Token, and Secret
// @Status = Text to be displayed in the tweet
// @ReplyTo = Tweet ID to reply to. 
// @Done = should be a callback with (err, tweetObj) 
var postReplyChain = function(user, status, replyTo, done){
    var leftover = null;
//    var username = oauth._encodeData('@' + user.twitter.username);
    var username = '@' + user.twitter.username;
    leftover = status.substring(280 - username.length);
    status = username + ' ' + status.substring(0, 280 - username.length-2);

    oauth.post('https://api.twitter.com/1.1/statuses/update.json',
        user.twitter.token,
        user.twitter.tokenSecret,
        {status: status, in_reply_to_status_id: 1052647238025891841},
        'application/json',
        function(err, results, res){
            if(err) { done(createTwitterError(err), null); }
            else {
                results = JSON.parse(results);
                console.log('Second Tweet');
                if(leftover && leftover !== ''){
                    postReplyChain(user, leftover, results.id, done)
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
    postReply: postReply
}

// Return a function to setup Authentcation
exports.Authentication = authenticate;