var passport = require('passport');

//var Itch = require('../implementation/itch.js');
var OAuth2 = require('oauth').OAuth2;

var clientKey = 'ed7685aa7e34847f2d91e6247df997bb';
var clientSecret = '20fce3317e3e586180fe3fbe7df7b70d4778a82c8977570e749fa40bef7736a1';
var callbackURL = 'http://localhost:' + (process.env.PORT || '3000') + '/auth/Itch/callback';

var oauth = new OAuth2(
    clientKey, 
    clientSecret,
    'https://itch.io/user/oauth',
    '', // Auth URL
    null // Token URL
);
oauth.useAuthorizationHeaderforGET(true);

//var base = 'https://www.googleapis.com/youtube/v3/';

// Sets up the Strategy to be ready for authentication
var authenticate = function(){
    passport.use(new Itch(
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
            
            user.itch = {};
            user.itch.id = profile.id;
            user.itch.token = accessToken;
            user.itch.tokenSecret = refreshToken;            
            
            done(null, user);        
        }
    ));
}

// Takes in a Itch response and returns a properly formated error.
var createItchError = function(err){
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

}

// Return a function to setup Authentcation
exports.Authentication = authenticate;