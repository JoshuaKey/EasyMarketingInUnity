// NOTICE -----------------------------------------------------------
// DO NOT DO WHAT I AM DOING
// I am sending a 400 Response with a status of 200
// I am doing this because C# Http Web Request is DUMB
// They throw an exception on a 400 status.
// You can get a response from the exception, but it doesnt contain the original response body
// BASICALY C# HTTPWEBREQUEST IS DUMB
// THIS IS NOT PROPER REST API
// DON'T DO THIS
var send200Response = function(res, result){
    res.status(200);
    var json = {
        status: 200,
        errorCode:  0,
        errorMessage: '',
        displayMessage: 'Success!',
        results: result
    };
    res.json(json);
}
var send400Response = function(res, errCode, errMessage){
    errCode = errCode || -1;
    errMessage = errMessage || 'Unknown';
    
    res.status(200); 
    var json = {
        status: 400,
        errorCode:  errCode,
        errorMessage: errMessage,
        displayMessage: 'Error',
        results: null
    };
    res.json(json);
}
var send403Response = function(res, provider){
    provide = provider || '???';
    
    res.status(200); 
    var json = {
        status: 403,
        errorCode:  1,
        errorMessage: 'User has not authenticated. Call cmd/' + provider + '/Authenticate first',
        displayMessage: 'Please authenticate the site first',
        results: null
    };
    res.json(json);
}
var send500Response = function(res, errCode, errMessage){ 
    errCode = errCode || -1;
    errMessage = errMessage || 'Unknown';
    
    res.status(200); 
    var json = {
        status: 500,
        errorCode:  errCode,
        errorMessage: errMessage,
        displayMessage: 'Something went wrong!',
        results: null
    };
    res.json(json);
}

// Bind this to Response with serviceCallback.bind(res);
var serviceCallback = function(err, obj){   
    console.log('Error: ', err);
    console.log('Results: ', obj);
    
    if(this.headersSent) { console.log("Response already Sent"); return; }
    
    if(err) { send400Response(this, err.code, err.message);  }
    else { send200Response(this, obj); }
}

module.exports = {
    send200Response: send200Response,
    send400Response: send400Response,
    send403Response: send403Response,
    send500Response: send500Response,
    serviceCallback: serviceCallback
}