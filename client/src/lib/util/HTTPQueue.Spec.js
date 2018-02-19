import { httpQueue } from './HTTPQueue';
import { expect } from 'chai';

describe('Http Queue', () => {

    it('should be able to subscribe to event', () => {

        return new Promise(done => {
            httpQueue.pushToBuffer({empty: 'now'});

            httpQueue
                .retryAll(config => {
                    expect(config).to.deep.equal({empty: 'now'});
                    return Promise.resolve({});
                })
                .then(done)
                .catch(done);
        });

    });

    it('should throw no representation if promise is empty', () => {
        return new Promise(done => {
            httpQueue.pushToBuffer({empty: 'now'});

            httpQueue
                .retryAll((config) => {
                    expect(config).to.deep.equal({empty: 'now'});
                    return Promise.resolve();
                })
                .catch(done);
        });
    });

});