import 'underscore';
import _ from './index';
import { expect } from 'chai';

describe('Async collection mixins', () => {

    describe('_.reduceWaitAll()', () => {

        it('should total up a list', () => {
            return _([1, 2])
                .reduceWaitAll((memo, i) => {
                    return Promise.resolve(memo + i);
                }, 0)
                .then(result => {
                    expect(result).to.equal(3);

                });

        });

        it('should total up using nested promises', () => {
            return _([1, 2])
                .reduceWaitAll((memo, i) => {

                    if (i === 2) {
                        return Promise.resolve(memo + i * i)
                            .then(m => m + 5);
                    }
                    return Promise.resolve(memo + i);
                }, 0)
                .then(result => {
                    expect(result).to.equal(1 + 2 * 2 + 5);

                });

        });

        it('should map across an object synchronise', () => {
            return _({state: 'this', title: 'that'})
                .mapObjectWaitAll((value, key) => {
                    if (key === 'state') {
                        return Promise.resolve('that');
                    }
                    return Promise.resolve(value);
                })
                .then(result => {
                    expect(result).to.deep.equal({state: 'that', title: 'that'});

                });
        });

        it('should map across an object aysnc', () => {
            const resource = {state: 'this', title: 'that'};
            return _(resource)
                .mapObjectWaitAll((value, key) => {
                    if (key === 'state') {
                        return Promise.resolve()
                            .then(() => 'that');
                    }
                    return Promise.resolve(value);
                })
                .then(result => {
                    expect(result).to.deep.equal({state: 'that', title: 'that'});
                    expect(result).not.to.equal(resource);

                });
        });

        it('should map across an object and replace keys', () => {
            const resource = {'question-item': 'this', title: 'that'};
            return _(resource)
                .mapObjectWaitAll(value => Promise.resolve(value), _.dashToCamel)
                .then(result => {
                    expect(result).to.deep.equal({questionItem: 'this', title: 'that'});
                    expect(result).not.to.equal(resource);

                });
        });
    });
});