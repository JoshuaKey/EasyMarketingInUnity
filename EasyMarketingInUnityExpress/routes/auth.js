var express = require('express');
var passport = require('passport');

var crypto = require("crypto");

var router = express.Router();

var twitterScope = {
    scope: ['read-write']
}
var facebookScope = {
    scope: ['user_posts', 'user_likes', 'user_photos', 'user_videos', 'manage_pages', 'publish_pages', 'publish_video', 'read_page_mailboxes', 'publish_to_groups'] 
}
var discordScope = {
    scope: ['identify', 'guilds', 'rpc', 'rpc.api', 'bot', 'messages.read'],
    permissions: 537386048 // View Channels, Add Reactions, Send MEssages, Embed Links, Attach Files, Read MEssage History, Mention Everyone, Use External Emojis, 
} 
var redditScope = {
    scope: ['identity', 'history', 'read', 'mysubreddits', 'submit', 'vote', 'flair'],
    duration: 'permanent',
    state: crypto.randomBytes(32).toString('hex')
}
//var youtubeScope = {
//    scope: ['https://www.googleapis.com/auth/youtube.readonly', 'https://www.googleapis.com/auth/youtube.upload', 'https://www.googleapis.com/auth/youtube']
//}
//var itchScope = {
//    scope: ['profile:me']
//}
var vkScope = {
    scope: ['groups', 'offline', 'photos', 'video', 'pages', 'status', 'wall'],
}

var slackAuth = false;
var slackScopeBasic = {
    scope: ['identity.basic', 'identity.team'],
}
var slackScopeAdvanced = {
    scope: ['channels:history', 'channels:read', 'groups:read', 'chat:write:user', 'files:write:user', 'stars:read', 'stars:write'],
}

var redirect = {
    successRedirect: '/success',
    failure: '/error/' 
};

/* GET home page. */
router.get('/Twitter/callback', passport.authenticate('twitter', redirect));
router.get('/Twitter', passport.authenticate('twitter', twitterScope));

router.get('/Facebook/callback', passport.authenticate('facebook', redirect));
router.get('/Facebook', passport.authenticate('facebook', facebookScope));

router.get('/Discord/callback', passport.authenticate('discord', redirect));
router.get('/Discord', passport.authenticate('discord', discordScope));

router.get('/Reddit/callback', passport.authenticate('reddit', redirect));
router.get('/Reddit', passport.authenticate('reddit', redditScope));

router.get('/Slack/callback', function(req, res ,next){
    if(slackAuth){
        slackAuth = false;
        passport.authenticate('slack', redirect)(req, res, next);       
    } else {
        slackAuth = true;
        passport.authenticate('slack', { successRedirect: '/auth/Slack/Advanced', failure: '/error/'})(req, res, next);        
    }
   
});
router.get('/Slack/Advanced', passport.authenticate('slack', slackScopeAdvanced));
router.get('/Slack', passport.authenticate('slack', slackScopeBasic));

//router.get('/Youtube/callback', passport.authenticate('youtube', redirect));
//router.get('/Youtube', passport.authenticate('youtube', youtubeScope));

//router.get('/Itch/callback', passport.authenticate('itch', redirect));
//router.get('/Itch', passport.authenticate('itch', itchScope));

router.get('/Vkontakte/callback', passport.authenticate('vkontakte', redirect));
router.get('/Vkontakte', passport.authenticate('vkontakte', vkScope));

module.exports = router;