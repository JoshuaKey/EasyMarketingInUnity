var passport = require('passport');

module.exports = function(app){
    
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