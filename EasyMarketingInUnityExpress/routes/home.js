var express = require('express');
var router = express.Router();

/* GET home page. */
router.get('/', function(req, res, next) {
  res.render('homePage');
});

router.get('/success', function(req, res, next) { 
    res.render('successPage'); 
});


module.exports = router;
