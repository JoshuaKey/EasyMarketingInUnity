var passport = require('passport');

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
}

// Because I open a Browser Window for Authentication, I must send the Browser's Cookie and Session ID to original
// altResponse is the Response Object from the cmd/.../Authenticate Request
// successCallback is a method for processing the authentication, session, and cookies.
var altResponse = { res: null };
var successCallback = function (res){
    if(altResponse.res == null) { return; }
    var response = {
        error: '',
        results: ''
    };
    
    // Recover Cookies
    try {
        if(res.req.cookies){
            console.log('Alt Req Session');
            console.log(altResponse.res.req.session)
            console.log('Curr Req Session');
            console.log(res.req.session);
             altResponse.res.req.session = res.req.session;
        } else {
            response.error = response.error + '; Cookies not found';
        }    
    } 
    catch(err){
        response.error = response.error + '; ' + err; 
    }

    if(response.error == ''){
        response.results = 'Authentication Successful';
    }

    altResponse.res.json(response);  
    altResponse.res = null;
}

exports.Service = {
    altResponse: altResponse,
    successCallback: successCallback
}

exports.Configure = configure;

