var express = require('express');
var passport = require('passport');

var router = express.Router();

var twitterScope = {
    scope: ['read-write']
}
var redirect = {
    successRedirect: '/success',
    failure: '/error/' 
};

/* GET home page. */
router.get('/Twitter/callback', passport.authenticate('twitter', redirect));
router.get('/Twitter', passport.authenticate('twitter', twitterScope));

module.exports = router;