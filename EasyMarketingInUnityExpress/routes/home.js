var express = require('express');

var router = express.Router();

var PassportService = require('../authentication/passport').Service;

/* GET home page. */
router.get('/', function(req, res, next) {
    res.render('homePage');
});

router.get('/success', function(req, res, next) { 
    // Call Success Callback
    PassportService.successCallback(res);
    
    res.render('successPage'); 
});

module.exports = router;
