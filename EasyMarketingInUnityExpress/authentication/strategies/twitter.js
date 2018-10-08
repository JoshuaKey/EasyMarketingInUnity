var passport = require('passport');
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
    oauth.get(
        'https://api.twitter.com/1.1/statuses/user_timeline.json?user_id=' + user.twitter.id,
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
// @Done = should be a callback with (err, tweetObj) (?)
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
    postTweet: postTweet
}

// Return a function to setup Authentcation
exports.Authentication = authenticate;