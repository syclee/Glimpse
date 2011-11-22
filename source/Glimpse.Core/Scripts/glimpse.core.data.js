data = (function () {
    var //Support
        inner = {}, 
        types = [],
        base = {},
    
        //Main 
        update = function (data) {
            inner = data;  
            pubsub.publish('action.data.update');
        },
        reset = function () {
            types = [];
            update(base);
        },
        retrieve = function (requestId, type, callback) { 
            if (type) {
                if (types.length == 0 || types[types.length - 1] != type)
                    types.push(type);
            }
            else
                types.pop();   

            if (callback && callback.start)
                callback.start(requestId);

            if (requestId != base.requestId) {
                $.ajax({
                    url : glimpsePath + 'History',
                    type : 'GET',
                    data : { 'ClientRequestID': requestId },
                    contentType : 'application/json',
                    success : function (result, textStatus, jqXHR) {   
                        if (callback && callback.success) { callback.success(requestId, result, inner, textStatus, jqXHR); }
                        update(result);  
                    }, 
                    complete : function (jqXHR, textStatus) {
                        if (callback && callback.complete) { callback.complete(requestId, jqXHR, textStatus); }
                    }
                });
            }
            else { 
                if (callback && callback.success) { callback.success(requestId, base, inner, 'Success'); }
                update(base);  
                if (callback && callback.complete) { callback.complete(requestId, undefined, 'Success'); } 
            }
        },

        current = function () {
            return inner;
        },
        currentTypes = function () {
            return types
        },
        currentMetadata = function () {
            return inner.metadata;
        },

        init = function () {
            inner = glimpseData; 
            base = glimpseData; 
        };
        
    init(); 
    
    return { 
        current : current,
        currentTypes : currentTypes,
        currentMetadata : currentMetadata,
        update : update,
        retrieve : retrieve,
        reset : reset
    };
}())