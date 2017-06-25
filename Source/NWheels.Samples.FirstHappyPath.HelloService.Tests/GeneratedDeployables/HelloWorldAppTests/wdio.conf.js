exports.config = {
    specs: [
        'tests/**'
    ],

    capabilities: [{
        browserName: 'chrome'
    }],

    framework: 'jasmine',

    mochaOpts: {
        ui: 'bdd',
        //compilers: ['js:babel-register']
    },

    // ...
    services: ['selenium-standalone'],
    // ...
};
