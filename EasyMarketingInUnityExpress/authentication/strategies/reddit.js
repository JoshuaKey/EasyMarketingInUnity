var passport = require('passport');
var fs = require('fs');
var mime = require('mime-types');
var path = require('path');

var Reddit = require('passport-reddit').Strategy;
var OAuth2 = require('oauth').OAuth2;

var clientKey = 'cYZ5uEfpGsPUWw';
var clientSecret = 'JiKX32KnzzRBc-XF3e6uSjWD32U';
var callbackURL = 'http://localhost:' + (process.env.PORT || '3000') + '/auth/Reddit/callback';

var oauth = new OAuth2(
    clientKey, 
    clientSecret,
    'https://www.reddit.com/api/v1/',
    'authorize',
    'access_token',
    {'User-Agent': 'PC:com.example.EasyMarketingInUnity:v0.1 (by /u/Flameo326)'}
);
oauth.useAuthorizationHeaderforGET(true);


// With Bearer token -> oauth https://oauth.reddit.com
// All bearer tokens expire after an hour, therefore need to use refresh to get new access token

var base = 'https://oauth.reddit.com/';
var base2 = 'https://www.reddit.com/';

// Sets up the Strategy to be ready for authentication
var authenticate = function(){
    passport.use(new Reddit(
        {
        clientID: clientKey,
        clientSecret: clientSecret,
        callbackURL: callbackURL,
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
            
            user.reddit = {};
            user.reddit.id = profile.id;
            user.reddit.token = accessToken;
            user.reddit.tokenSecret = refreshToken;
            user.reddit.refreshTime = Date.now() + 60 * 60 * 1000; 
            console.log('Refresh Time: ', Date.now(), ' ', user.reddit.refreshTime);
            
            user.reddit.username = profile.name;
            
            done(null, user);        
        }
    ));
}

var checkRefresh = function(user, done){
    done = done || (function() {});
    if(Date.now() < user.reddit.refreshTime){
        done();
        return;
    }
    
    // May need to change 'client_id' -> user and secret -> password
    var code = user.reddit.tokenSecret;
    var params = {
        grant_type: 'refresh_token',
        client_id: 'user',
        client_secret: 'password'
    };
    var redditStrat = passport._strategy('reddit');
    redditStrat.refresh(code, params, 
        function(err, accessToken, refreshToken, results){
            if(err){
                console.log(err);
                done();
                return;
            }
        
            user.reddit.token = accessToken;
            if(refreshToken){
                 user.reddit.tokenSecret = refreshToken;
            }
            user.reddit.refreshTime = Date.now() + 60 * 60 * 1000; 
        
            console.log(user);
            console.log(results);
        
            done();
        }
    );
}

// @User = User Object containing Facebook data
// @Done = should be a callback with (err, res) (?)
var getPosts = function(user, done){
    var url = base + 'user/' + user.reddit.username + '/submitted';
    
    oauth.get(url,
        user.reddit.token,
        function(err, results) {
            if(err) { done(createRedditError(err), null); return; }

            results = JSON.parse(results);
            
        
            //results = results.filter(post => post["data"]["children"][0]["kind"] == "t3");
        
            //results.data.likes = upvote, downvote or neutral
            //results.data.id = ID36 of object
            done(null, results);
        }
    );
}

// @User = User Object containing Facebook data
// @Done = should be a callback with (err, res) (?)
var getSubscribed = function(user, done){
    var url = base + '/subreddits/mine/subscriber';
    
    oauth.get(url,
        user.reddit.token,
        function(err, results) {
            if(err) { done(createRedditError(err), null); return; }

            results = JSON.parse(results);
            //results.data.likes = upvote, downvote or neutral
            //results.data.id = ID36 of object
            done(null, results);
        }
    );
}

// @User = User Object containing Facebook data
// @Done = should be a callback with (err, res) (?)
var getComments = function(user, subreddit, article, done){ 
    var url = base + 'r/' + subreddit + '/comments/' + article + '?limit=100&depth=1';
    
    console.log(url);
    
    oauth.get(url,
        user.reddit.token,
        function(err, results) {
            if(err) { done(createRedditError(err), null); return; }

            results = JSON.parse(results);
            done(null, results);
        }
    );
}

// @User = User Object containing Facebook data
// @Done = should be a callback with (err, res) (?)
var getFlairs = function(user, subreddit, done){
    var url = base + 'r/' + subreddit + '/api/flairselector?name=' + oauth._encodeData('u/' + user.reddit.username);;
    
    console.log(url);
    
    oauth.get(url,
        user.reddit.token,
        function(err, results) {
            if(err) { done(createRedditError(err), null); return; }

            results = JSON.parse(results);
            done(null, results);
        }
    );
}

// @User = User Object containing Facebook data
// @Done = should be a callback with (err, res) (?)
var postThread = function(user, subreddit, title, message, flair, done){
    var url = base + '/api/submit';
    
    oauth.post(url,
        user.reddit.token,
        {api_type: 'json', kind: 'self', sr: subreddit, title: title, text: message, flair: flair},
        '',
        function(err, results) {
            if(err) { done(createRedditError(err), null); return; }

            results = JSON.parse(results);
            done(null, results);
        }
    );
}


// @User = User Object containing Facebook data
// @Done = should be a callback with (err, res) (?)
var postComment = function(user, parent, message, done){
    var url = base + '/api/comment';
    
    oauth.post(url,
        user.reddit.token,
        {api_type: 'json', text: message, thing_id: parent},
        '',
        function(err, results) {
            if(err) { done(createRedditError(err), null); return; }

            results = JSON.parse(results);
            done(null, results);
        }
    );
}

// @User = User Object containing Facebook data
// @Id = ID of the thread or comment to vote on
// @Value = Int value of -1, 0, or 1 indicating a downvote, neutral or upvote
// @Done = should be a callback with (err, res) (?)
var postVote = function(user, id, value, done){
    var url = base + 'api/vote';
    
    var obj = {api_type: 'json', dir: value, id: id, };

    oauth.post(url,
        user.reddit.token,
        obj,
        '',
        function(err, results) {
            if(err) { done(createRedditError(err), null); return; }

            results = JSON.parse(results);
            done(null, results);
        }
    );
}

// Takes in a Twitter response and returns a properly formated error.
var createRedditError = function(err){
    // Example Error Response
//    {"message": "Forbidden", "error": 403}
    
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
    checkRefresh: checkRefresh, 
    
    getSubscribed: getSubscribed,
    getPosts: getPosts,
    getComments: getComments,
    getFlairs: getFlairs,

    postThread: postThread,
    postComment: postComment,
    postVote: postVote,
}

// Return a function to setup Authentcation
exports.Authentication = authenticate;