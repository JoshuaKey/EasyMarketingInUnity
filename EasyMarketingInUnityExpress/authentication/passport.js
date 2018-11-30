var passport = require('passport');

var Res = require('../utility/response');

var configure = function(app){
    
    app.use(passport.initialize());
    app.use(passport.session());
    
    passport.serializeUser(function(user, done){
        console.log('Serializing User');
        console.log(user);
        done(null, user);
    });
    passport.deserializeUser(function(user, done){
        console.log('Deserializing User');
        console.log(user);
        done(null, user);  
    });
    
    require('./strategies/twitter').Authentication();
    require('./strategies/facebook').Authentication();
    require('./strategies/discord').Authentication();
    require('./strategies/reddit').Authentication();
    require('./strategies/slack').Authentication();
//    require('./strategies/youtube').Authentication();
//    require('./strategies/itch').Authentication();
    require('./strategies/vkontakte').Authentication();
}

// Because I open a Browser Window for Authentication, I must send the Browser's Cookie and Session ID to original
// altResponse is the Response Object from the cmd/.../Authenticate Request
// successCallback is a method for processing the authentication, session, and cookies.
var altResponse = { res: null };
var successCallback = function (res){
    if(altResponse.res == null) { return; }

    // Recover Session / Cookies
    try {
        if(res.req.cookies){
            // Store the original Session and the new Session
            if(altResponse.res.req.session && altResponse.res.req.session.passport){
                 res.req.session.passport.user = Object.assign(
                     altResponse.res.req.session.passport.user, 
                     res.req.session.passport.user);
            }
            
            altResponse.res.req.session = res.req.session;
            res.req.session = null;
            
            Res.send200Response(altResponse.res, 'Authentication Successful');
        } else {
            Res.send400Response(altResponse.res, -1, 'Session not found');
        }    
    } 
    catch(err){
        Res.send500Response(altResponse.res, -1, err.toString());
    }

    altResponse.res = null;
}

exports.Service = {
    altResponse: altResponse,
    successCallback: successCallback
}

exports.Configure = configure;

