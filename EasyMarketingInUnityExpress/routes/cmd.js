var express = require('express');
var opn = require('opn');

var router = express.Router();

var PassportService = require('../authentication/passport').Service;
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
////    req.session.clear();
//    req.session.destroy();
//    res.clearCookie('connect.sid');
//    req.logout();
//    sendResponse(res, null, 'Success');
    
    console.log('Shut Down Server???');
});

// TWITTER ----------------------------------------------------------------------------------
// status = Tweet Status (150 characters)
// media = Appended Image (5 MB), Gif (15 MB), Video (15 MB)
router.all('/Twitter/Post', function(req, res, next) {
    if(!req.user || !req.user.twitter){
        sendResponse(res, createAuthError('Please Authenticate Twitter'), null);
        return;
    }
    
    if(!req.query.status){     
        sendResponse(res, createQueryError('Missing status Query Parameter'), null);       
        return;
    } 
    
    var status = req.query.status;
    var media = req.query.media; 
    
    console.log(media);
    
    if(media){
        res.connection.setTimeout(0);
        TwitterService.postMedia(req.user, status, media, function(err, tweet){
            console.log(err);
            console.log(tweet);
            sendResponse(res, err, tweet);    
        });
    } else {
        TwitterService.postTweet(req.user, status, function(err, tweet){
            console.log(err);
            console.log(tweet);
            sendResponse(res, err, tweet);    
        });
    }

    //
});
router.all('/Twitter/Get', function(req, res, next) {    
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
    
    if(req.user && req.user.twitter){
        sendResponse(res, null, 'Authentication Successful');
        return;
    }

    opn('http://localhost:' + (process.env.PORT || '3000') + '/auth/Twitter');   
    
    // Save the Original Response
    PassportService.altResponse.res = res;
});

module.exports = router;