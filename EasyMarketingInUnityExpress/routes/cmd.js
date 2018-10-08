var express = require('express');
var opn = require('opn');

var router = express.Router();

var TwitterService = require('../authentication/strategies/twitter').Service;
var sendResponse = function(res, error, results){
    var response = {
        error: error,
        results: results
    };

    res.json(response);        
}

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

// TWITTER ----------------------------------------------------------------------------------
router.get('/Twitter/Post', function(req, res, next) {
    if(!req.user || !req.user.twitter){
        sendResponse(res, createAuthError('Please Authenticate Twitter'), null);
        return;
    }
    
    if(!req.query.status){     
        sendResponse(res, createQueryError('Missing status Query Parameter'), null);       
        return;
    } 
    
    var status = req.query.status;
    
    TwitterService.postTweet(req.user, status, function(err, tweet){
        console.log(err);
        console.log(tweet);
        sendResponse(res, err, tweet);    
    });
    
});
router.get('/Twitter/Get', function(req, res, next) {
    if(!req.user || !req.user.twitter){
        sendResponse(res, createAuthError('Please Authenticate Twitter'), null);
        return;
    }
    
    console.log(res);
    
    TwitterService.getTweet(req.user, function(err, tweet){
        console.log(err);
        console.log(tweet);
        sendResponse(res, err, tweet);        
    });
    
});
router.get('/Twitter/Authenticate', function(req, res, next) {
    res.connection.setTimeout(0); // Don't Timeout, make take a while
    
    var promise = opn('http://localhost:' + (process.env.PORT || '3000') + '/auth/Twitter', { wait: true });   
    promise.then((value) => {
        sendResponse(res, '', '');
    });
});

module.exports = router;