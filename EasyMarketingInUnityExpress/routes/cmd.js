var express = require('express');
var opn = require('opn');

var router = express.Router();

var PassportService = require('../authentication/passport').Service;
var TwitterService = require('../authentication/strategies/twitter').Service;
var FacebookService = require('../authentication/strategies/facebook').Service;
var DiscordService = require('../authentication/strategies/discord').Service;
var RedditService = require('../authentication/strategies/reddit').Service;
var SlackService = require('../authentication/strategies/slack').Service;
//var YoutubeService = require('../authentication/strategies/youtube').Service;
//var ItchService = require('../authentication/strategies/itch').Service;
var VKService = require('../authentication/strategies/vkontakte').Service;
var Res = require('../utility/response');

router.all('/Shutdown', function(req, res, next){
    console.log('Shut Down Server???');
    
    DiscordService.disconnect();
    
    Res.send200Response(res, 'Shutdown Successful (Not Implemented)');
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
    var like = req.query.like;
       
    if(!status && !media){   
        Res.send400Response(res, 2, 'Missing "status" or "media" Query Parameter.');     
        return;
    }   
    
    if(like && !status){
        Res.send400Response(res, 2, 
            'Parameter "like" is defined. Parameter "status" is not defined. Please Include both in Query');     
        return;
    }

    if(like){
        TwitterService.postLike(req.user, status, Res.serviceCallback.bind(res));
    } else if(media) {
        res.connection.setTimeout(0);
        TwitterService.postMedia(req.user, status, media, multiple, Res.serviceCallback.bind(res));
    } else if(replyTo) {
        //Res.send500Response(res, 20, 'Not Implemented');
        TwitterService.postReply(req.user, status, replyTo, multiple, Res.serviceCallback.bind(res));
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

// FACEBOOK ----------------------------------------------------------------------------------
router.all('/Facebook/Post', function(req, res, next) {
    // Check for Authentication
    if(!req.user || !req.user.facebook){
        Res.send403Response(res, 'Facebook');
        return;
    }
    
    // Check for Query
    var message = req.query.message;
    var pageID = req.query.pageID;
    var like = req.query.like;
    
    if(!message && !like){
        Res.send400Response(res, 2, 'Missing "message" or "like" Query Parameter');     
        return;
    }
    if(like && !pageID){
        Res.send400Response(res, 2, 
            'Parameter "like" is defined. Parameter "pageID" is not defined. Please Include both in Query');     
        return;
    }
    
    if(like){
        FacebookService.postLike(req.user, pageID, Res.serviceCallback.bind(res));
    } else if(pageID){
        FacebookService.postPageMessage(req.user, message, pageID, Res.serviceCallback.bind(res));
    } else {
        FacebookService.postMessage(req.user, message, Res.serviceCallback.bind(res));
    }  
});
router.all('/Facebook/Get', function(req, res, next) {  
    // Check for Authentication
    if(!req.user || !req.user.facebook){
        Res.send403Response(res, 'Facebook');
        return;
    }  
    
    // Check for Query
    var accountIndex = req.query.accountIndex;
    var profileID = req.query.profileID || req.user.facebook.id;

     if(accountIndex){
        FacebookService.getAccounts(req.user, accountIndex, Res.serviceCallback.bind(res));
    } else {
        FacebookService.getFeed(req.user, profileID, Res.serviceCallback.bind(res));      
    }

});
router.all('/Facebook/Authenticate', function(req, res, next) {
    if(req.user && req.user.facebook){
        Res.send200Response(res, 'User already authenticated Facebook');
        return;
    }
    
    res.connection.setTimeout(0); // Don't Timeout, make take a while

    opn('http://localhost:' + (process.env.PORT || '3000') + '/auth/Facebook');   
    
    // Save the Original Response
    PassportService.altResponse.res = res;
});

// DISCORD ----------------------------------------------------------------------------------
router.all('/Discord/Post', function(req, res, next) {
    // Check for Authentication
    if(!req.user || !req.user.discord){
        Res.send403Response(res, 'Discord');
        return;
    }
    
    // Check for Query
    var channel = req.query.channel;
    var message = req.query.message;
    var file = req.query.file;
    var like = req.query.like;
    
    if(!channel){
        Res.send400Response(res, 2, 'Missing "channel" Query Parameter');     
        return; 
    }
    if(!message && !file){
        Res.send400Response(res, 2, 'Missing "message" or "file" Query Parameter');     
        return;
    }
    if(like && (!message || !channel)){
        Res.send400Response(res, 2, 
            'Parameter "like" is defined. Parameter "message" or "channel" is not defined. Please Include both in Query');     
        return;
    }
    
    DiscordService.checkRefresh(req.user);
     
    if(like){
        DiscordService.postLike(req.user, channel, message, Res.serviceCallback.bind(res));
    } else {      
        DiscordService.getHook(req.user, channel, 
            function(err, results){
                if(err) { Res.serviceCallback.bind(res)(err,null); return; }
        
                if(file) {
                    DiscordService.postFile(req.user, results, file, Res.serviceCallback.bind(res));    
                }  
                if(message){
                    DiscordService.postMessage(req.user, results, message, Res.serviceCallback.bind(res));
                }
            }
        )   
    } 
});
router.all('/Discord/Get', function(req, res, next) {  
    // Check for Authentication
    if(!req.user || !req.user.discord){
        Res.send403Response(res, 'Discord');
        return;
    }  
    
    // Check for Query
    var channel = req.query.channel;
    var message = req.query.message;
    var like = req.query.like;
    
    if(like && (!message || !channel)){
        Res.send400Response(res, 2, 
            'Parameter "like" is defined. Parameter "message" or "channel" is not defined. Please Include both in Query');     
        return;
    }
    
    DiscordService.checkRefresh(req.user);
    
    if(like){
        DiscordService.getLike(req.user, channel, message, Res.serviceCallback.bind(res));
    } else if(channel){
        DiscordService.getMessages(req.user, channel, Res.serviceCallback.bind(res));
    } else {
        DiscordService.getChannels(req.user, Res.serviceCallback.bind(res));
    } 
});
router.all('/Discord/Authenticate', function(req, res, next) {
    if(req.user && req.user.discord){
        Res.send200Response(res, 'User already authenticated Discord');
        return;
    }
    
    res.connection.setTimeout(0); // Don't Timeout, make take a while

    opn('http://localhost:' + (process.env.PORT || '3000') + '/auth/Discord');   
    
    // Save the Original Response
    PassportService.altResponse.res = res;
});

// REDDIT ----------------------------------------------------------------------------------
router.all('/Reddit/Post', function(req, res, next) {
    // Check for Authentication
    if(!req.user || !req.user.reddit){
        Res.send403Response(res, 'Reddit');
        return;
    }
    
    // Check for Query
    var objID = req.query.objID;
    
    var vote = req.query.vote;
    
    var isComment = req.query.isComment;
    
    var message = req.query.message;
    var title = req.query.title;
    var flair = req.query.flair;
    
    if(!objID){
        Res.send400Response(res, 2, 'Missing "objID" Query Parameter. This refers to a subreddit, post, or comment depending on other query parameters');     
        return; 
    }
    if(isComment && !message){
        Res.send400Response(res, 2, 
            'Parameter "isComment" is defined. Parameter "message" is not defined. Please Include both in Query');     
        return;
    }
    
    if(message && !title){
        title = message.substring(0, 120);
    }
            
    RedditService.checkRefresh(req.user);
    
    if(vote){
        RedditService.postVote(req.user, objID, vote, Res.serviceCallback.bind(res));
    } else if(isComment){
        RedditService.postComment(req.user, objID, message, Res.serviceCallback.bind(res));
    } else {
        RedditService.postThread(req.user, objID, title, message, flair, Res.serviceCallback.bind(res));
    }   
});
router.all('/Reddit/Get', function(req, res, next) {  
    // Check for Authentication
    if(!req.user || !req.user.reddit){
        Res.send403Response(res, 'Reddit');
        return;
    }  
    
    // Check for Query
    var subscribed = req.query.subscribed;
    var subreddit = req.query.subreddit;
    var article = req.query.article;
    
//    if((subreddit || article) && !(subreddit && article)){
//        Res.send400Response(res, 2, 
//            'Both Parameter "subreddit" and "article" should be defined. Please Include both in Query');     
//        return;
//    }
    
    RedditService.checkRefresh(req.user);

    if(subscribed){
        RedditService.getSubscribed(req.user, Res.serviceCallback.bind(res));
    } else if(article){
        RedditService.getComments(req.user, subreddit, article, Res.serviceCallback.bind(res));
    } else if(subreddit){
        RedditService.getFlairs(req.user, subreddit, Res.serviceCallback.bind(res));
    } else {
        RedditService.getPosts(req.user, Res.serviceCallback.bind(res));
    }
    
});
router.all('/Reddit/Authenticate', function(req, res, next) {
    if(req.user && req.user.reddit){
        Res.send200Response(res, 'User already authenticated Reddit');
        return;
    }
    
    res.connection.setTimeout(0); // Don't Timeout, may take a while

    opn('http://localhost:' + (process.env.PORT || '3000') + '/auth/Reddit');   
    
    // Save the Original Response
    PassportService.altResponse.res = res;
});

// SLACK ----------------------------------------------------------------------------------
router.all('/Slack/Post', function(req, res, next) {
    // Check for Authentication
    if(!req.user || !req.user.slack){
        Res.send403Response(res, 'Slack');
        return;
    }
    
    // Check for Query
    var channel = req.query.channel;
    var message = req.query.message;
    var file = req.query.file;
    var timestamp = req.query.timestamp; // Indicates A specific message or Starring

    if(!channel){
        Res.send400Response(res, 2, 'Missing "channel" Query Parameter.');     
        return; 
    }
    if(!message && !timestamp && !file){
        Res.send400Response(res, 2, 'Missing "message" or "timestamp" or "file" Query Parameter. Please include one.');     
        return; 
    }
    
    if(timestamp){
        SlackService.postLike(req.user, channel, timestamp, Res.serviceCallback.bind(res));
    } else if(file){
        SlackService.postFile(req.user, channel, file, message, Res.serviceCallback.bind(res));           
    }else { // Message Only
        SlackService.postMessage(req.user, channel, message, Res.serviceCallback.bind(res));
    }

});
router.all('/Slack/Get', function(req, res, next) {  
    // Check for Authentication
    if(!req.user || !req.user.slack){
        Res.send403Response(res, 'Slack');
        return;
    }  
    
    // Check for Query
    var channel = req.query.channel;
    
    if(channel){
        SlackService.getMessages(req.user, channel, Res.serviceCallback.bind(res));
    } else {
        SlackService.getChannels(req.user, Res.serviceCallback.bind(res));
    }
});
router.all('/Slack/Authenticate', function(req, res, next) {
    if(req.user && req.user.slack){
        Res.send200Response(res, 'User already authenticated Slack');
        return;
    }
    
    res.connection.setTimeout(0); // Don't Timeout, may take a while

    opn('http://localhost:' + (process.env.PORT || '3000') + '/auth/Slack');   
    
    // Save the Original Response
    PassportService.altResponse.res = res;
});

// YOUTUBE ----------------------------------------------------------------------------------
/*
router.all('/Youtube/Post', function(req, res, next) {
    // Check for Authentication
    if(!req.user || !req.user.youtube){
        Res.send403Response(res, 'Youtube');
        return;
    }
    
    // Check for Query

    YoutubeService.checkRefresh(req.user);

});
router.all('/Youtube/Get', function(req, res, next) {  
    // Check for Authentication
    if(!req.user || !req.user.youtube){
        Res.send403Response(res, 'Youtube');
        return;
    }  
    
    // Check for Query
    
    
    YoutubeService.checkRefresh(req.user);
    
    YoutubeService.getActivity(req.user, Res.serviceCallback.bind(res));  
});
router.all('/Youtube/Authenticate', function(req, res, next) {
    if(req.user && req.user.youtube){
        Res.send200Response(res, 'User already authenticated Youtube');
        return;
    }
    
    res.connection.setTimeout(0); // Don't Timeout, may take a while

    opn('http://localhost:' + (process.env.PORT || '3000') + '/auth/Youtube');   
    
    // Save the Original Response
    PassportService.altResponse.res = res;
});
*/

// ITCH.IO ----------------------------------------------------------------------------------
/*
router.all('/Itch/Post', function(req, res, next) {
    // Check for Authentication
    if(!req.user || !req.user.itch){
        Res.send403Response(res, 'Itch.io');
        return;
    }
    
    // Check for Query

});
router.all('/Itch/Get', function(req, res, next) {  
    // Check for Authentication
    if(!req.user || !req.user.itch){
        Res.send403Response(res, 'Itch.io');
        return;
    }  
    
    // Check for Query

});
router.all('/Itch/Authenticate', function(req, res, next) {
    if(req.user && req.user.itch){
        Res.send200Response(res, 'User already authenticated Itch');
        return;
    }
    
    res.connection.setTimeout(0); // Don't Timeout, may take a while

    opn('http://localhost:' + (process.env.PORT || '3000') + '/auth/Itch');   
    
    // Save the Original Response
    PassportService.altResponse.res = res;
});
*/

// VKONTAKTE ----------------------------------------------------------------------------------
router.all('/Vkontakte/Post', function(req, res, next) {
    // Check for Authentication
    if(!req.user || !req.user.vkontakte){
        Res.send403Response(res, 'Vkontakte');
        return;
    }
    
    // Check for Query
    var message = req.query.message;
    var postID = req.query.postID;
    var like = req.query.like;
    
    if(!message){
        Res.send400Response(res, 2, 'Parameter "message" is not defined.');     
        return;
    }
    if(like && !postID){
        Res.send400Response(res, 2, 
            'Parameter "like" is defined. Parameter "postID" is not defined. Please Include both in Query');     
        return;
    }
    
    if(like){
        VKService.postLike(req.user, postID, message, Res.serviceCallback.bind(res));
    } else if (postID){
        VKService.postComment(req.user, postID, message, Res.serviceCallback.bind(res));
    } else {
        VKService.postMessage(req.user, message, Res.serviceCallback.bind(res));
    }
});
router.all('/Vkontakte/Get', function(req, res, next) {  
    // Check for Authentication
    if(!req.user || !req.user.vkontakte){
        Res.send403Response(res, 'Vkontakte');
        return;
    }  
    
    // Check for Query
    var postID = req.query.postID;
    var type = req.query.type;
    
    if(type && !postID){
        Res.send400Response(res, 2, 
            'Parameter "likes" is defined. Parameter "postID" is not defined. Please Include both in Query');     
        return;
    }
    
    if(type){
        VKService.getLike(req.user, postID, type, Res.serviceCallback.bind(res));
    } else if (postID){
        VKService.getComments(req.user, postID, Res.serviceCallback.bind(res)); 
    } else {
        VKService.getWall(req.user, Res.serviceCallback.bind(res));
    }
    
});
router.all('/Vkontakte/Authenticate', function(req, res, next) {
    if(req.user && req.user.vkontakte){
        Res.send200Response(res, 'User already authenticated Vkontakte');
        return;
    }
    
    res.connection.setTimeout(0); // Don't Timeout, may take a while

    opn('http://localhost:' + (process.env.PORT || '3000') + '/auth/Vkontakte');   
    
    // Save the Original Response
    PassportService.altResponse.res = res;
});


router.all('/Temp', function(req, res, next) {
    res.redirect('https://oauth.vk.com/authorize?response_type=token&redirect_uri=http%3A%2F%2Flocalhost%3A3000%2Fauth%2FVkontakte%2Fcallback&client_id=6752179');
    
//https://oauth.vk.com/authorize?response_type=code&redirect_uri=http%3A%2F%2Flocalhost%3A3000%2Fauth%2FVkontakte%2Fcallback&scope=groups%2Coffline%2Cphotos%2Cvideo%2Cpages%2Cstatus%2Cwall&client_id=6752179
//https://oauth.vk.com/authorize?response_type=token&redirect_uri=http%3A%2F%2Flocalhost%3A3000%2Fauth%2FVkontakte%2Fcallback&scope=groups%2Coffline%2Cphotos%2Cvideo%2Cpages%2Cstatus%2Cwall&client_id=6752179
    
//https://oauth.vk.com/authorize?response_type=token&redirect_uri=http%3A%2F%2Flocalhost%3A3000%2Fauth%2FVkontakte%2Fcallback&scope=groups%2Coffline%2Cphotos%2Cvideo%2Cpages%2Cstatus%2Cwall&client_id=6758168
})

module.exports = router;