var express = require('express');

var router = express.Router();

var PassportService = require('../authentication/passport').Service;

/* GET home page. */
router.get('/', function(req, res, next) {
    res.render('homePage');
});

router.get('/success', function(req, res, next) { 
    
    // Call Success Callback
    console.log(req.session);
    console.log(res);
    PassportService.successCallback(res);
    
//    TwitterService.authObj.res2 = res;
//    TwitterService.authCallback();// Grab the Cookies and Session ID from this res2 and give it to Res1!
    
//    console.log('Inside Success Page');
//    console.log(req.sessionID);
//    console.log(req.user);
//    console.log();
    
    res.render('successPage'); 
});

module.exports = router;
