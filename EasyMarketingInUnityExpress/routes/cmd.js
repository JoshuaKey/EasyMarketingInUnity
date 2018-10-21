var express = require('express');
var opn = require('opn');

var router = express.Router();

var PassportService = require('../authentication/passport').Service;
var TwitterService = require('../authentication/strategies/twitter').Service;
var Res = require('../utility/response');

router.all('/Shutdown', function(req, res, next){
    console.log('Shut Down Server???');
    Res.send200Response(res, 'Shutdown Successful');
});

// TWITTER ----------------------------------------------------------------------------------
router.all('/Twitter/Post', function(req, res, next) {
    // Check for Authentication
    if(!req.user || !req.user.twitter){
        Res.send403Response(res, 'Twitter');
        return;
    }
    
    // Check for Query
    var status = req.query.status; // Tweet Status
    var multiple = req.query.multiple; // Whether to make multiple posts
    var media = req.query.media; // Appended Media File
    var replyTo = req.query.replyTo; // Tweet ID to reply to
    if(!status){   
        Res.send400Response(res, 2, 'Missing "status" Query Parameter');     
        return;
    }    

    if(media) {
        res.connection.setTimeout(0);
        TwitterService.postMedia(req.user, status, media, multiple, Res.serviceCallback.bind(res));
    } else if(replyTo) {
        Res.send500Response(res, 20, 'Not Implemented');
//            TwitterService.postReply(req.user, status, replyTo, multiple, Res.serviceCallback.bind(res));
    } else {
        TwitterService.postTweet(req.user, status, multiple, Res.serviceCallback.bind(res));
    }
});

router.all('/Twitter/Get', function(req, res, next) {  
    // Check for Authentication
    if(!req.user || !req.user.twitter){
        Res.send403Response(res, 'Twitter');
        return;
    }  
    
    // Check for Query
    var tweet_id = req.query.tweet_id; // Tweet ID string
    var reply = req.query.reply; // Whether to search for replies
    if(reply && !tweet_id){   
        Res.send400Response(res, 2, 
            'Parameter "reply" is defined. Parameter "tweet_id" is not defined. Please Include both in Query');     
        return;
    }
    
    if(reply){
        TwitterService.getReplies(req.user, tweet_id, Res.serviceCallback.bind(res));
    } else if(tweet_id){
        TwitterService.getTweet(req.user, tweet_id, Res.serviceCallback.bind(res));
    } else {
        TwitterService.getTimeline(req.user, Res.serviceCallback.bind(res));
    }
});
router.all('/Twitter/Authenticate', function(req, res, next) {
    if(req.user && req.user.twitter){
        Res.send200Response(res, 'User already authenticated Twitter');
        return;
    }
    
    res.connection.setTimeout(0); // Don't Timeout, make take a while

    opn('http://localhost:' + (process.env.PORT || '3000') + '/auth/Twitter');   
    
    // Save the Original Response
    PassportService.altResponse.res = res;
});

module.exports = router;