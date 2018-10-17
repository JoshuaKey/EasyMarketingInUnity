var passport = require('passport');
var fs = require('fs');
var mime = require('mime-types');

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
            
            user.twitter = {};
            user.twitter.id = profile.id;
            user.twitter.token = token;
            user.twitter.tokenSecret = tokenSecret;
            
            done(null, user);        
        }
    ));
}

// Sends an oAuth call to get the last tweet posted.
// Limit of 900 calls per 15 minutes
// @User = User Object containing Twitter ID, Token, and Secret
// @Done = should be a callback with (err, tweetObj) (?)
var getTweet = function(user, done){
    oauth.get('https://api.twitter.com/1.1/statuses/user_timeline.json?user_id=' + user.twitter.id,
        user.twitter.token,
        user.twitter.tokenSecret,
        function(err, results, res){
            if(err) { done(err, null); }
            else {
                results = JSON.parse(results);
                done(null, results[0]);
            }
        }
    );
}

// Sends an oAuth call to post a tweet.
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
    
//Resolution should be <= 1280x1080 (width x height)
//Number of frames <= 350
//Number of pixels (width * height * num_frames) <= 300 million
//File size <= 15Mb
        
//Duration should be between 0.5 seconds and 30 seconds (sync) / 140 seconds (async)
//File size should not exceed 15 mb (sync) / 512 mb (async)
//Dimensions should be between 32x32 and 1280x1024
        
        // Type: multipart/form-data OR application/x-www-form-urlencoded
        // URL: https://upload.twitter.com/1.1/media/upload.json
        
        // I must make an INIT Post with the Media MetaData
//'command': 'INIT',
//'media_type': 'video/mp4',
//'total_bytes': self.total_bytes,
//'media_category': 'tweet_video'
        
        // Followed by APPEND withe Data
//file = open(self.video_filename, 'rb')
//
//while bytes_sent < self.total_bytes:
//chunk = file.read(4*1024*1024)
//
//print('APPEND')
//
//request_data = {
//'command': 'APPEND',
//'media_id': self.media_id,
//'segment_index': segment_id
//}
//
//files = {
//'media':chunk
//}
//
//req = requests.post(url=MEDIA_ENDPOINT_URL, data=request_data, files=files, auth=oauth)
//
//if req.status_code < 200 or req.status_code > 299:
        
        // And FINALIZE when completely uploaded
//'command': 'FINALIZE',
//'media_id': self.media_id
//req = requests.get(url=MEDIA_ENDPOINT_URL, params=request_params, auth=oauth)
        
        // I can check the STATUS
//request_params = {
//'command': 'STATUS',
//'media_id': self.media_id
//}
//
//req = requests.get(url=MEDIA_ENDPOINT_URL, params=request_params, auth=oauth)
        
//        fs.readFile("data/wx.hourly.txt", "utf8", function(err, data){
//            if(err) throw err;
//
//            var resultArray = //do operation on data that generates say resultArray;
//
//            res.send(resultArray);
//        });
        
//        var data = '';
//
//        var readStream = fs.createReadStream('my-file.txt', 'utf8');
//
//        readStream.on('data', function(chunk) {  
//            data += chunk;
//        }).on('end', function() {
//            console.log(data);
//        });
    
    console.log("In Post Media()");
    var url = 'https://upload.twitter.com/1.1/media/upload.json';
    var media_id;
    var stats;

    
    // Post INIT 
    fs.stat(media, function(err, _stats) {
        if(err) { done(err, null); return; }
        
        console.log("In Init()");
        
        stats = _stats;
        
        var total_bytes = stats.size;
        var media_type = mime.lookup(media);

        var query = '?command=INIT&total_bytes=' + total_bytes + '&media_type=' + media_type;

//        post(url + query, Append);
        post(url + query, (err, tweet) => {
            if(err) { done(err, null); return; }
        
            tweet = JSON.parse(tweet);
            done(null, tweet);
        });
    });

    // Post APPEND and Loop
    function Append(err, tweet){
        if(err) { done(err, null); return; }
        
        console.log("In Append()");
        
        tweet = JSON.parse(tweet);
        media_id = tweet.media_id_string; // 64 bit number / string
        
        console.log(tweet);
        
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
                console.log(query);
                console.log(data);
                
                post(url + query, (err, tweet) => {
                    console.log(tweet);
                    if(err) { 
                       readStream.destroy(err);
                    }
                });
            } 
        }
        
// RETURNS 200-299 Status 
    }

    // Post FINALIZE
    function Finalize(){
        console.log("In Finalize()");
        
        var query = '?command=FINALIZE&media_id=' + media_id;
        post(url + query, Status);
     
// RETURNS
//// Example of sync FINALIZE response
//{
//  "media_id": 710511363345354753,
//  "media_id_string": "710511363345354753",
//  "size": 11065,
//  "expires_after_secs": 86400,
//  "video": {
//    "video_type": "video/mp4"
//  }
//}
//
//// Example of async FINALIZE response which requires further STATUS command call(s)
//{
//  "media_id": 710511363345354753,
//  "media_id_string": "710511363345354753",
//  "expires_after_secs": 86400,
//  "size": 10240,
//  "processing_info": {
//    "state": "pending",
//    "check_after_secs": 5 // check after 5 seconds for update using STATUS command
//  }
//}
    }
    
    // POST STATUS
    function Status(err, tweet){
        if(err) { done(err, null); return; }
        tweet = JSON.parse(tweet);
        
        console.log(tweet);
        
        if(tweet.processing_info){
            if(tweet.process_info.error) { 
                done(tweet.process_info.error, null); 
            } else if(tweet.process_info.state === "suceeded"){
                Tweet(err, tweet);
            } else {
                var check_after_secs = tweet.processing_info.check_after_secs;
//                setTimeout(Status, check_after_secs*1000, err, tweet);/// Example was 5, Thats a long time...
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


// RETURNS
//// Example of an in_progress response:
//
//{
//  "media_id":710511363345354753,
//  "media_id_string":"710511363345354753",
//  "expires_after_secs":3595,
//  "processing_info":{
//    "state":"in_progress", // state transition flow is pending -> in_progress -> [failed|succeeded]
//    "check_after_secs":10, // check for the update after 10 seconds
//    "progress_percent":8 // Optional [0-100] int value. Please don't use it as a replacement of "state" field.
//  }
//}
//
//// Example of a failed response:
//
//{
//  "media_id":710511363345354753,
//  "media_id_string":"710511363345354753",
//  "processing_info":{
//    "state":"failed",
//    "progress_percent":12,
//    "error":{
//      "code":1,
//      "name":"InvalidMedia",
//      "message":"Unsupported video format"
//    }
//  }
//}
//
//// Example of a succeeded response:
//
//{
//  "media_id":710511363345354753,
//  "media_id_string":"710511363345354753",
//  "expires_after_secs":3593,
//  "video":{
//    "video_type":"video/mp4"
//  },
//  "processing_info":{
//    "state":"succeeded",
//    "progress_percent":100,
//  }
//}
    }     

    ///Post TWEET
    function Tweet(err, tweet){
        if(err) { done(err, null); return; }
        tweet = JSON.parse(tweet);
        
        console.log(tweet); 
       
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

var postMediaSingle = function(user, status, media, done){   
    function post(url, done){
        oauth.post(url,
            user.twitter.token,
            user.twitter.tokenSecret,
            '',
            'application/x-www-form-urlencoded',
            done
        );
    }
}

// Sends an oAuth call to post a tweet.
// Limit of 300 per 3 hours
// @User = User Object containing Twitter ID, Token, and Secret
// @Status = Text to be displayed in the tweet
// @Done = should be a callback with (err, tweetObj) 
var postTweet = function(user, status, done){
    oauth.post('https://api.twitter.com/1.1/statuses/update.json?status='+status,
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

// Return an object with service methods to make oAuth calls
exports.Service = {
    getTweet: getTweet,
    postTweet: postTweet,
    postMedia: postMediaSingle
}

// Return a function to setup Authentcation
exports.Authentication = authenticate;