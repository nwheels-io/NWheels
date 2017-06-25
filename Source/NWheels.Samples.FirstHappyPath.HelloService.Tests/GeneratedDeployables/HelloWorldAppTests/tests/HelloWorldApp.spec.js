const webdriverio = require('webdriverio');
const should = require('should');
const spawn = require('child_process').spawn;
const path  = require('path');
const fs = require('fs');

describe('HelloWorldApp', function () {
    let service = null;

    beforeAll(function() {
        const dllPath = findMicroserviceDllPath();

        return new Promise(function (resolve, reject) {
            let started = false;
            service = spawn('dotnet', [dllPath]);
            service.stdout.on('data', (data) => {
                if (data && data.indexOf('Microservice is up.') >= 0) {
                    console.log('Microservice started.');
                    started = true;
                    resolve();
                }
            });
            service.stderr.on('data', (data) => {
                if (!started) {
                    console.log('ERROR: Microservice failed to start!', data);
                    reject(data);
                } else {
                    console.log('ERROR: Microservice > ', data);
                }
            });
        });
    });

    it('can navigate to start page', function() {
        browser.url('http://localhost:5000');
        let title = browser.getTitle();
        title.should.be.equal('Hello World App');
    });

    it('can execute Hello transaction', function() {
        browser.setValue('#Transaction_ViewModel_Name__Input', 'WebDriverIO');
        browser.waitForEnabled('#Transaction_SubmitCommand__Button', 1000);
        browser.click('#Transaction_SubmitCommand__Button');
        browser.waitForVisible('#Transaction_ViewModel_Message__Text', 1000);

        let text = browser.getText('#Transaction_ViewModel_Message__Text');
        text.should.be.equal('Hello world, from WebDriverIO!');
    });

	it("initially GO button is disabled", function() {
        browser.waitForEnabled('#Transaction_SubmitCommand__Button', 100, true);
    });
    
	it("emptying the Name field renders input form invalid", function() {
        browser.setValue('#Transaction_ViewModel_Name__Input', 'AAA');
        browser.waitForEnabled('#Transaction_SubmitCommand__Button', 1000);
        browser.setValue('#Transaction_ViewModel_Name__Input', '\b\b\b');
        browser.waitForEnabled('#Transaction_SubmitCommand__Button', 1000, true);
        browser.waitForVisible('#Transaction_ViewModel_Name__Input-error', 1000);
    });

	it("typing into the Name field makes input form valid", function() {
        browser.setValue('#Transaction_ViewModel_Name__Input', 'AAA');
        browser.waitForEnabled('#Transaction_SubmitCommand__Button', 1000);
        browser.setValue('#Transaction_ViewModel_Name__Input', '\b\b\b');
        browser.waitForEnabled('#Transaction_SubmitCommand__Button', 1000, true);
        browser.setValue('#Transaction_ViewModel_Name__Input', 'BBB');
        browser.waitForEnabled('#Transaction_SubmitCommand__Button', 1000);
        browser.waitForVisible('#Transaction_ViewModel_Name__Input-error', 1000, true);
    });
    
    afterAll(function () {
        return new Promise(function (resolve, reject) {
            service.on('exit', (code) => {
                console.log(`Microservice exited with code ${code}`);
                if (code == 0) {
                    resolve();
                } else {
                    reject();
                }
            });
            service.stdin.write("\r\n");
            service.stdin.end(); 
        });
    });

    function findMicroserviceDllPath() {
        const configPlaceholder = '##CFG##';
        const serviceRelativePath = '../../../../'; 
        const servicePathFormat = `NWheels.Samples.FirstHappyPath.HelloService/bin/${configPlaceholder}/netcoreapp1.1/hello.dll`;

        for (var configName of ['Release', 'Debug']) {
            let probePath = path.join(__dirname, serviceRelativePath, servicePathFormat.replace(configPlaceholder, configName));

            console.log('Looking for microservice DLL at: ', probePath);
            
            if (fs.existsSync(probePath)) {
                console.log('Found microservice DLL at: ', probePath);
                return probePath;
            }
        }

        throw new Error('Microservice DLL could not be found');
    }
});
