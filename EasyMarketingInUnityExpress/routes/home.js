var express = require('express');

var router = express.Router();

var TwitterService = require('../authentication/strategies/twitter').Service;

/* GET home page. */
router.get('/', function(req, res, next) {
    console.log('Inside the homepage callback function');
    console.log(req.sessionID);
    res.render('homePage');
});

router.get('/success', function(req, res, next) { 
    TwitterService.authObj.res2 = res;
    TwitterService.authCallback();// Grab the Cookies and Session ID from this res2 and give it to Res1!
    
    res.render('successPage'); 
});


module.exports = router;
