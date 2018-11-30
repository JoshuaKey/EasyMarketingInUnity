
//
///** 
// * Dependencies
// */
//var OAuth2Strategy = require('passport-oauth2');
//var Error = require('passport-oauth2').InternalOAuthError;
//var util = require('util');
//
///**
// * `Strategy` constructor.
// *
// * Go to https://itch.io/user/settings/oauth-apps to create an Itch.io App
// *
// * Applications must supply a `verify` callback which accepts an `accessToken`,
// * `refreshToken` and service-specific `profile`, and then calls the `cb`
// * callback supplying a `user`, which should be set to `false` if the
// * credentials are not valid. If an exception occured, `err` should be set.
// *
// * Options:
// *   - `clientID`       Client ID
// *   - `clientSecret`   Client Secret
// *   - `callbackURL`    Redirect URL
// *   - `scope`          Array of permission scopes to request
// *                      Check the official documentation for valid scopes to pass as an array.
// * 
// * @constructor
// * @param {object} options
// * @param {function} verify
// * @access public
// */
//function Strategy(options, verify) {
//    options = options || {};
//    options.authorizationURL = options.authorizationURL || 'https://itch.io/user/oauth';
//    options.tokenURL = options.tokenURL || 'https://itch.io/user/oauth';
//    options.scopeSeparator = options.scopeSeparator || ' ';
//    
//// https://itch.io/user/oauth?client_id=ed7685aa7e34847f2d91e6247df997bb&scope=profile%3Ame&response_type=token&redirect_uri=http%3A%2F%2Flocalhost%3A3000%2Fauth%2FItch%2Fcallback
//
//    OAuth2Strategy.call(this, options, verify);
//    this.name = 'itch';
//    this._oauth2.useAuthorizationHeaderforGET(true);
//}
//
///**
// * Inherits from `OAuth2Strategy`
// */
//util.inherits(Strategy, OAuth2Strategy);
//
///**
// * Retrieve user profile from service provider.
// *
// * OAuth 2.0-based authentication strategies can overrride this function in
// * order to load the user's profile from the service provider.  This assists
// * applications (and users of those applications) in the initial registration
// * process by automatically submitting required information.
// *
// * @param {String} accessToken
// * @param {Function} done
// * @api protected
// */
//Strategy.prototype.userProfile = function(accessToken, done) {
//    var self = this;
//    this._oauth2.get('https://itch.io/api/1/key/me', accessToken, function(err, body, res) {
//        var profile = {};
//        profile.provider = 'itch';
//        profile.accessToken = accessToken;
//        profile._raw = body;
//        
//        if (err) {
//            return done(new InternalOAuthError('Failed to fetch the user profile.', err), profile)
//        }
//
//        try {
//            var json = JSON.parse(body);
//        }
//        catch (e) {
//            return done(new Error('Failed to parse the user profile.'), profile);
//        }
//
//        profile = Object.assign(parsedData, profile);
//        profile._json = json;
//
//        done(null, profile);
//    });
//};
//
///**
// * Expose `Strategy`.
// */
//module.exports = Strategy;
//

// I might have to do all this manually.
// 1.) On auth/Itch -> https://itch.io/user/oauth, so a function(req, res, next) { ... }
// 2.) Need the Client ID, response_type=token, redirect_url, scope
// 3.) On Authorize
// 4.) On Fail
