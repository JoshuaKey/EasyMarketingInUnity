var createError = require('http-errors');
var express = require('express');
var session = require('express-session');
var path = require('path');
var cookieParser = require('cookie-parser');
var httpLogger = require('morgan');
var logger = require('./utility/logger');
var uuid = require('uuid/v4');
var FileStore = require('session-file-store')(session);

var homeRouter = require('./routes/home');
var authRouter = require('./routes/auth');
var cmdRouter = require('./routes/cmd');

var app = express();
var secret = 'ILoveMarketing';

logger.info('\n------------------------------------------------------');
logger.info('Starting server on port ' + (process.env.PORT || '3000'));

// view engine setup
app.set('views', path.join(__dirname, 'views'));
app.set('view engine', 'ejs');

app.use(httpLogger('dev'));
app.use(express.json());
app.use(express.urlencoded({ extended: false }));
app.use(cookieParser());
app.use(express.static(path.join(__dirname, 'public')));

app.use(session({
    genid: (req) => {
        logger.info('Inside the session middleware: ' + req.sessionID);
        return uuid(); // use UUIDs for session IDs
    },
    store: new FileStore(),
    secret: 'keyboard cat',
    resave: false,
    saveUninitialized: true
}));
require('./authentication/passport')(app);

app.use('/', homeRouter);
app.use('/auth', authRouter);
app.use('/cmd', cmdRouter);

// catch 404 and forward to error handler
app.use(function(req, res, next) {
  next(createError(404));
});

// error handler
app.use(function(err, req, res, next) {
  // set locals, only providing error in development
  res.locals.message = err.message;
  res.locals.error = req.app.get('env') === 'development' ? err : {};
    
  logger.error(err);

  // render the error page
  res.status(err.status || 500);
  res.render('errorPage', {error: err});
});

module.exports = app;
