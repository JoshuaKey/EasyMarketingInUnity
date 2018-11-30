var passport = require('passport');

var Youtube = require('passport-youtube-v3').Strategy;
var OAuth2 = require('oauth').OAuth2;

var clientKey = '451439906987-aiqc5jcjplk25rp952bpp77phk35hb9s.apps.googleusercontent.com';
var clientSecret = 'L3MYeMGliuRoQ3ugDWtIxway';
var callbackURL = 'http://localhost:' + (process.env.PORT || '3000') + '/auth/Youtube/callback';

var oauth = new OAuth2(
    clientKey, 
    clientSecret,
    'https://accounts.google.com/o/oauth2/',
    'auth',
    'token',
);
oauth.useAuthorizationHeaderforGET(true);

var base = 'https://www.googleapis.com/youtube/v3/';

// Sets up the Strategy to be ready for authentication
var authenticate = function(){
    passport.use(new Youtube(
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
            
            user.youtube = {};
            user.youtube.id = profile.id;
            user.youtube.token = accessToken;
            user.youtube.tokenSecret = refreshToken;   
            
            user.youtube.refreshTime = Date.now() + profile.expires_in;
            
            done(null, user);        
        }
    ));
}

var checkRefresh = function(user, done){
    done = done || (function() {});
    if(Date.now() < user.youtube.refreshTime){
        done();
        return;
    }
    
    // May need to change 'client_id' -> user and secret -> password
    var code = user.youtube.tokenSecret;
    var params = {
        grant_type: 'refresh_token',
    };
    var strat = passport._strategy('youtube');
    strat.refresh(code, params, 
        function(err, accessToken, refreshToken, results){
            if(err){
                console.log(err);
                done();
                return;
            }
        
            user.youtube.token = accessToken;
            if(refreshToken){
                 user.youtube.tokenSecret = refreshToken;
            }
            user.youtube.refreshTime =  Date.now() + results.expires_in; 
        
            console.log(user);
            console.log(results);
        
            done();
        }
    );
}

var getActivity = function(user, done){
    var url = base + 'activities?maxResults=50&mine=true&part=snippet,contentDetails';

    oauth.get(url,
        user.youtube.token,
        function(err, results) {
            if(err) { done(createYoutubeError(err), null); return; }

            results = JSON.parse(results);
            done(null, results);
        }
    );
}

//youtube.activities.list
//youtube.videos.list

//youtube.videos.rate

//youtube.activities.insert
//youtube.videos.insertw

// Takes in a Youtube response and returns a properly formated error.
var createYoutubeError = function(err){
    // Example Error Response
    //'{"error":{"errors":[{"domain":"global","reason":"required","message":"Required parameter: part","locationType":"parameter","location":"part"}],"code":400,"message":"Required parameter: part"}}'
    
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
    
    getActivity: getActivity
}

// Return a function to setup Authentcation
exports.Authentication = authenticate;