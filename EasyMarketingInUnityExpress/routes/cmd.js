var express = require('express');
var opn = require('opn');

var router = express.Router();

var TwitterService = require('../authentication/strategies/twitter').Service;

// Error Information
// 1 - Authentication Problem
// 2 - Query Problem
var createAuthError = function(message){
    message = message || 'Please Authenticate first';
    return {
        code: 1,
        message: message
    }
}
var createQueryError = function(message){
    message = message || 'Incorrect Query Parameters';
    
    return {
        code: 2,
        message: message
    }
}

var sendResponse = function(res, error, results){
    var response = {
        error: error,
        results: results
    };

    res.json(response);        
}

router.all('/Shutdown', function(req, res, next){
//    req.session.clear();
    req.session.destroy();
    res.clearCookie('connect.sid');
    req.logout();
    sendResponse(res, null, 'Success');
    
    console.log('Shut Down Server???');
});

// TWITTER ----------------------------------------------------------------------------------
router.all('/Twitter/Post', function(req, res, next) {
    var user = req.user;
    console.log(req);
    console.log(user);
    
    if(!user || !user.twitter){
        sendResponse(res, createAuthError('Please Authenticate Twitter'), null);
        return;
    }
    
    if(!req.query.status){     
        sendResponse(res, createQueryError('Missing status Query Parameter'), null);       
        return;
    } 
    
    var status = req.query.status;
    
    TwitterService.postTweet(user, status, function(err, tweet){
        console.log(err);
        console.log(tweet);
        sendResponse(res, err, tweet);    
    });
    
});
router.all('/Twitter/Get', function(req, res, next) {
//    req.session.reload(function(err) {
//      // session updated
//    })
    console.log(req);
    if(!req.user || !req.user.twitter){
        sendResponse(res, createAuthError('Please Authenticate Twitter'), null);
        return;
    }
    
    TwitterService.getTweet(req.user, function(err, tweet){
        console.log(err);
        console.log(tweet);
        sendResponse(res, err, tweet);        
    });
    
});
router.all('/Twitter/Authenticate', function(req, res, next) {
    res.connection.setTimeout(0); // Don't Timeout, make take a while
    
//    console.log(req);
//    console.log(res);
//    req.session.reload(function(err) {
//      // session updated
//    })
    
    console.log('Inside Authenticate Method');
    console.log(req.sessionID);

    opn('http://localhost:' + (process.env.PORT || '3000') + '/auth/Twitter');   
    
    TwitterService.authObj.res1 = res;
//    sendResponse(res, '', '');
});

module.exports = router;