import {expect} from 'chai';
import {log} from 'semantic-link/lib/logger';
import sinon from 'sinon';
import Loader from './Loader';

describe('Add a request', () => {

    let loader;

    beforeEach(() => {
        loader = new Loader();
        expect(loader.requests).to.be.empty;
    });

    it('should amke a single request passing through the id and config and clear the queue', () => {

        const config = {url: 1};

        const caller = sinon.stub(loader, 'schedule');
        caller.withArgs({id: config.url}, config).returns(Promise.resolve(true));

        return loader
            .request(config)
            .then(result => {
                log.debug('Result done');
                expect(result).to.equal(true);
                expect(caller.callCount).to.equal(1);
                expect(loader.requests[config]).to.be.undefined;
                caller.restore();
            });
    });

    it('should only make one schedule request across the same id', () => {

        const config = {url: 1};

        const caller = sinon.stub(loader, 'schedule');
        caller.returns(
            new Promise(resolve => {
                setTimeout(() => resolve({response: true}), 20);
            })
        );

        return Promise.all(
            [
                loader.request(config),
                loader.request(config),
                loader.request(config),
            ])
            .then(([first, second, third]) => {
                log.debug('Result done');
                expect(first.response).to.equal(true);
                expect(second.response).to.equal(true);
                expect(third.response).to.equal(true);
                expect(caller.callCount).to.equal(1);
                expect(loader.requests[config]).to.be.undefined;
                caller.restore();

            });


    });


    it('should make one request per unique id across multiple requests', () => {

        const caller = sinon.stub(loader, 'schedule');
        caller.returns(
            new Promise(resolve => {
                setTimeout(() => resolve({response: true}), 20);
            })
        );

        return Promise.all(
            [
                loader.request({url: 1}),
                loader.request({url: 1}),
                loader.request({url: 2}),
                loader.request({url: 3}),
                loader.request({url: 2}),
                loader.request({url: 3}),
            ])
            .then(([first, second, third, fourth, fifth, sixth]) => {
                log.debug('Result done');
                expect(first.response).to.equal(true);
                expect(second.response).to.equal(true);
                expect(third.response).to.equal(true);
                expect(fourth.response).to.equal(true);
                expect(fifth.response).to.equal(true);
                expect(sixth.response).to.equal(true);
                expect(caller.callCount).to.equal(3);
                expect(loader.requests[1]).to.be.undefined;
                expect(loader.requests[2]).to.be.undefined;
                expect(loader.requests[3]).to.be.undefined;
                caller.restore();

            });


    });


});
