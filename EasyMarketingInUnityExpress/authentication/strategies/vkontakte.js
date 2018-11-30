var passport = require('passport');

var Vkontakte = require('passport-vkontakte').Strategy;
var OAuth2 = require('oauth').OAuth2;

var clientKey = '6758168';
var clientSecret = 'u8OPcMm2MKHpz7ssdZcy';
//var clientKey = '6752179';
//var clientSecret = 'n0fE2Q6BqFxnXTiDwqTH';
var callbackURL = 'http://localhost:' + (process.env.PORT || '3000') + '/auth/Vkontakte/callback';

var oauth = new OAuth2(
    clientKey, 
    clientSecret,
    'https://oauth.vk.com/',
    'authorize',
    'access_token',
);
//oauth.useAuthorizationHeaderforGET(true);

var base = 'https://api.vk.com/method/';

// Sets up the Strategy to be ready for authentication
var authenticate = function(){
    passport.use(new Vkontakte(
        {
        clientID: clientKey,
        clientSecret: clientSecret,
        callbackURL: callbackURL,
        explicit: false,
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
            
            user.vkontakte = {};
            user.vkontakte.id = profile.id;
            user.vkontakte.token = accessToken;
            user.vkontakte.tokenSecret = refreshToken;
            
            done(null, user);        
        }
    ));
}

// Add an Explicit or Implicit Check...

var getWall = function(user, done){
    var url = 'https://api.vk.com/method/wall.get?owner_id='+ user.vkontakte.id + '&filter=owner&&v=5.87';

    oauth.get(url,
        user.vkontakte.token,
        function(err, results) {
            if(err) { done(createVKError(err), null); return; }

            results = JSON.parse(results);
            if (results.error) {done(createVKError(results.error), null); return;}
        
            done(null, results);
        }
    );
}

var getComments = function(user, postID, done){
    var url = 'https://api.vk.com/method/wall.getComments?owner_id='+ user.vkontakte.id + '&post_id=' + postID + '&need_likes=1&count=50&v=5.87';

    oauth.get(url,
        user.vkontakte.token,
        function(err, results) {
            if(err) { done(createVKError(err), null); return; }

            results = JSON.parse(results);
            if (results.error) {done(createVKError(results.error), null); return;}
        
            done(null, results);
        }
    );
}

var getLike = function(user, postID, type, done){
//    var url = 'https://api.vk.com/method/likes.isLiked?user_id='+ user.vkontakte.id + '&filter=owner&&v=5.87';
    var url = 'https://api.vk.com/method/likes.isLiked?item_id=' + postID + '&type=' + type + '&v=5.87';

    oauth.get(url,
        user.vkontakte.token,
        function(err, results) {
            if(err) { done(createVKError(err), null); return; }

            results = JSON.parse(results);
            if (results.error) {done(createVKError(results.error), null); return;}
        
            done(null, results);
        }
    );
}

var postMessage = function(user, message, done){
    var url = 'https://api.vk.com/method/wall.post?owner_id' + user.vkontakte.id + '&message=' + message + '&v=5.87';

    oauth.get(url,
        user.vkontakte.token,
        function(err, results) {
            if(err) { done(createVKError(err), null); return; }

            results = JSON.parse(results);
            if (results.error) {done(createVKError(results.error), null); return;}

            done(null, results);
        }
    );

}

var postComment = function(user, postID, message, done){
    var url = 'https://api.vk.com/method/wall.createComment?owner_id' + user.vkontakte.id + '&post_id=' +postID + '&message=' + message + '&v=5.87';

    oauth.get(url,
        user.vkontakte.token,
        function(err, results) {
            if(err) { done(createVKError(err), null); return; }

            results = JSON.parse(results);
            if (results.error) {done(createVKError(results.error), null); return;}

            done(null, results);
        }
    );

}

var postLike = function(user, postID, type, done){
    
    getLike(user, postID, function(err, results){
        var url
        if(results.liked){
            url = 'https://api.vk.com/method/likes.delete?item_id=' + postID + '&type=' + type + '&v=5.87';
        } else {
            url = 'https://api.vk.com/method/likes.add?item_id=' + postID + '&type=' + type + '&v=5.87';
        }
        
        oauth.get(url,
            user.vkontakte.token,
            function(err, results) {
                if(err) { done(createVKError(err), null); return; }

                results = JSON.parse(results);
                if (results.error) {done(createVKError(results.error), null); return;}

                done(null, results);
            }
        );
    });

}

// likes.add
// status.set
// board.addtopic
// board.createcommetn
// board.getcomment
// board.gettopics
// fave.getposts
// groups.get
// likes.isLiked

// Takes in a VK response and returns a properly formated error.
var createVKError = function(err){
    // Example Error Response
// { error:
//   { error_code: 5,
//     error_msg: 'User authorization failed: no access_token passed.', 
//     request_params: [ [Object], [Object], [Object] ] } }
    
    console.log(err);
    var data;
    if(typeof err !== 'object'){
        data = JSON.parse(err.data);
    } else {
        data = err;
    }
 
    
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
    getWall: getWall,
    getComments: getComments,
    getLike: getLike,
    
    postMessage: postMessage,
    postComment: postComment,
    postLike : postLike
}

// Return a function to setup Authentcation
exports.Authentication = authenticate;