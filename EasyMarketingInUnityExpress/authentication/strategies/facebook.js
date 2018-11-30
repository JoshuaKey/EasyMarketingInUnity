var passport = require('passport');
var fs = require('fs');
var mime = require('mime-types');
var path = require('path');

var Facebook = require('passport-facebook').Strategy;
var OAuth2 = require('oauth').OAuth2;

var clientKey = '512891192510961';
var clientSecret = 'b30a21bbdd973c3a3c1343cc3fa60d04';
var callbackURL = 'http://localhost:' + (process.env.PORT || '3000') + '/auth/Facebook/callback';
var oauth = new OAuth2(
    clientKey, 
    clientSecret,
    'https://graph.facebook.com',
    null,
    'oauth2/token'   
);

// Sets up the Strategy to be ready for authentication
var authenticate = function(){
    passport.use(new Facebook(
        {
        clientID: clientKey,
        clientSecret: clientSecret,
        callbackURL: callbackURL,
        passReqToCallback: true,
        profileFields: ['id', 'displayName']
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
            
            user.facebook = {};
            user.facebook.id = profile.id;
            user.facebook.token = accessToken;
            user.facebook.tokenSecret = refreshToken;
            
            user.facebook.username = profile.displayName;
            user.facebook.accounts = {};
            
            done(null, user);        
        }
    ));
}

// Returns the specific Tweet in detail
// @User = User Object containing Facebook data
// @ID = Facebook Profile ID, defaults to User ID
// @Done = should be a callback with (err, tweetObj) (?)
var getFeed = function(user, ID, done){
    if(typeof ID == "function"){
        done = ID;
        ID = user.facebook.id;
    }
    
    var base = 'https://graph.facebook.com/v3.2/' + ID + '/posts';//.summary(false)
    
    oauth.get(base,
        user.facebook.token,
        function(err, results){
            if(err) { done(createFacebookError(err), null); return; }
            
            results = JSON.parse(results);
            results = results.data.filter(x => x.message);
            done(null, results);
        }
    );
    // returns an object called Data, with coresponding Data, like an array.
}

var getAccounts = function(user, index, done){
    var url;
    if(index < 0 && user.facebook.accounts.prev) {
        url = user.facebook.accounts.prev;
    } else if(index > 0 && user.facebook.accounts.next){
         url = user.facebook.accounts.next;
    } else {
        if(user.facebook.accounts.curr){
            url = user.facebook.accounts.curr;
        } else {
            url = 'https://graph.facebook.com/v3.2/' + user.facebook.id + '/accounts';
        }
    }

    oauth.get(url,
        user.facebook.token,
        function(err, results){
            if(err) { done(createFacebookError(err), null); return; }
            
            results = JSON.parse(results);
            user.facebook.accounts.prev = results.paging.previous;
            user.facebook.accounts.next = results.paging.next;
            user.facebook.accounts.curr = url;
            done(null, results);
        }
    );
}

//var getLike = function(user, postID, done){
//    if(typeof postID == "function"){
//        done = postID;
//        postID = user.facebook.id;
//    }
//    var url = 'https://graph.facebook.com/v3.2/' + postID + '/accounts';
//
//    oauth.get(url,
//        user.facebook.token,
//        function(err, results){
//            if(err) { done(createFacebookError(err), null); return; }
//            
//            results = JSON.parse(results);
//            done(null, results);
//        }
//    );
//}

// Returns the specific Tweet in detail
// @User = User Object containing Facebook data
// @Done = should be a callback with (err, tweetObj) (?)
var postMessage = function(user, message, done){
    var base = 'https://graph.facebook.com/v3.2/me/feed';
    var query = '?message=' + message;
    
    oauth.post(base,
        user.facebook.token,
        { message: message },
        '',
        function(err, results){
            if(err) { done(createFacebookError(err), null); return; }
            
            results = JSON.parse(results);
            done(null, results);
        }
    );
}

var postPageMessage = function(user, message, pageID, done){
    var base = 'https://graph.facebook.com/v3.2/' + pageID + '/feed';
    var query = '?message=' + message;
    
    oauth.post(base,
        user.facebook.token,
        { message: message },
        '',
        function(err, results){
            if(err) { done(createFacebookError(err), null); return; }
            
            results = JSON.parse(results);
            done(null, results);
        }
    );
}

var postLike = function(user, objID, done){
    var url = 'https://graph.facebook.com/v3.2/' + objID + '/reactions';//?fields=likes.summary(true)
    
    oauth.get(url,
        user.facebook.token,
        function(err, results){
            if(err) { done(createFacebookError(err), null); return; }
            
            results = JSON.parse(results);
            console.log(results);
            toggle(results.data.type == "LIKE");
        }
    );
    
    function toggle(liked){
        if(liked){
            oauth.post(url,
                user.facebook.token,
                '', 
                '',
                function(err, results){
                    if(err) { done(createFacebookError(err), null); return; }

                    results = JSON.parse(results);
                    done(null, results);
                }
            );
        } else {
            oauth.delete(url,
                user.facebook.token,
                function(err, results){
                    if(err) { done(createFacebookError(err), null); return; }

                    results = JSON.parse(results);
                    done(null, results);
                }
            );
        }
    }
}

// Takes in a Twitter response and returns a properly formated error.
var createFacebookError = function(res){
    // Example Error Response
//    { statusCode: 400,
//  data:
//   '{"error":{"message":"Unsupported post request. Object with ID \'1909633502435072\' does not exist, cannot be loaded due to missing permissions, or does not support this operation. Please read the Graph API documentation at https:\\/\\/developers.facebook.com\\/docs\\/graph-api","type":"GraphMethodException","code":100,"error_subcode":33,"fbtrace_id":"Eof0WSQydlq"}}' }

    console.log(res);
    var data = JSON.parse(res.data).error;
    
    var error = {
        code: 0,
        message: ''
    };
    
    error.code = 4;
    error.message = data.type + ' (' + data.code + '-' + data.error_subcode + '): ' + data.message;   
    
    return error;
}

// Return an object with service methods to make oAuth calls
exports.Service = {
    getFeed: getFeed,
    getAccounts: getAccounts,
    postMessage: postMessage,
    postPageMessage: postPageMessage,
    postLike: postLike
}

// Return a function to setup Authentcation
exports.Authentication = authenticate;