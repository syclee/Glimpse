module("glimpse.core.process");

test("Basic requirements", function() {
    expect(2);
    
    pubsub.publish('state.build.shell');
    var sRegistry = pubsub.sRegistry();  
    var pRegistry = pubsub.pRegistry();  

    ok(sRegistry['state.build.shell'].length == 1); 
    ok(pRegistry['data.elements.processed'].length == 2); 
});