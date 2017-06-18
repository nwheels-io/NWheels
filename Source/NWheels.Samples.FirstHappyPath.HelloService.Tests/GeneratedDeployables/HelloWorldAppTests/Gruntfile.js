'use strict';

module.exports = function (grunt) {

    grunt.initConfig({
        webdriver: {
            test: {
                configFile: './wdio.conf.js'
            }
        },
    });

    grunt.loadNpmTasks('grunt-webdriver');
    grunt.registerTask('e2e', ['webdriver']);
};
