import {expect} from 'chai';
import sinon from 'sinon';
import * as sync from './syncLinkedRepresentation';
import * as cache from '../cache/cache';
import {log} from 'logger';

global.Element = () => {
};

describe('Synchroniser', () => {

    it('should load sync', () => {
        expect(sync).to.not.be.null;
    });

    describe('getResource', () => {

        const resource = {
            links: [
                {
                    rel: 'self',
                    href: 'https://api.example.com/tenant/90a936d4a3'
                },
                {
                    rel: 'edit-form',
                    href: 'https://api.example.com/tenant/form/edit'
                }
            ],
            code: 'rewire.example.nz',
            name: 'Rewire NZ',
            description: 'A sample tenant (company/organisation)'
        };
        const editForm = {
            links: [
                {
                    rel: 'self',
                    href: 'https://api.example.com/tenant/form/edit'
                }
            ],
            items: [
                {
                    type: 'http://types/text',
                    name: 'code',
                    required: true,
                    description: 'The unique name in domain name format'
                },
                {
                    type: 'http://types/text',
                    name: 'name',
                    description: 'The name of the tenant to be shown on the screen'
                },
                {
                    type: 'http://types/text',
                    name: 'description',
                    description: 'Other details about the organisation'
                }
            ]
        };


        it('should not need updates when the same', () => {

            const get = sinon.stub();
            const post = sinon.stub();
            const put = sinon.stub();
            const del = sinon.stub();

            const document = resource;

            get.onCall(0).callsFake(() => {
                log.info('[Test] GET resource');
                return Promise.resolve({data: resource});
            });
            get.onCall(1).callsFake(() => {
                log.info('[Test] GET edit form');
                return Promise.resolve({data: editForm});
            });

            const sparseResource = cache.makeSparseCollectionResourceFromUri('https://api.example.com/tenant/90a936d4a3');

            return sync.getResource(sparseResource, document, [], {
                getFactory: get,
                postFactory: post,
                putFactory: put,
                deleteFactory: del
            })
                .then(result => {
                    expect(result).to.not.be.undefined;
                    expect(get.callCount).to.eq(2);
                    expect(post.callCount).to.eq(0);
                    expect(put.callCount).to.eq(0);
                    expect(del.callCount).to.eq(0);
                });
        });

        it('should make update when different attributes on the logically same resource', () => {
            const get = sinon.stub();
            const post = sinon.stub();
            const put = sinon.stub();
            const del = sinon.stub();

            const document = {
                links: [
                    {
                        rel: 'self',
                        href: 'https://api.example.com/tenant/90a936d4a3'
                    },
                    {
                        rel: 'edit-form',
                        href: 'https://api.example.com/tenant/form/edit'
                    }
                ],
                code: 'rewire.example.nz (updated)',
                name: 'Rewire NZ (updated)',
                description: 'A sample tenant (company/organisation)'
            };


            get.onCall(0).callsFake(() => {
                log.info('[Test] GET resource');
                return Promise.resolve({data: resource});
            });

            get.onCall(1).callsFake(() => {
                log.info('[Test] GET edit form');
                return Promise.resolve({data: editForm});
            });

            put.onFirstCall().callsFake(() => {
                log.info('[Test] PUT update');
                return Promise.resolve({headers: {location: 'https://api.example.com/tenant/90a936d4a3'}});
            });

            const sparseResource = cache.makeSparseResourceFromUri('https://api.example.com/tenant/90a936d4a3');

            return sync.getResource(sparseResource, document, [], {
                getFactory: get,
                postFactory: post,
                putFactory: put,
                deleteFactory: del
            })
                .then(result => {
                    expect(result).to.not.be.undefined;
                    expect(get.callCount).to.eq(2);
                    expect(put.callCount).to.eq(1);
                    expect(post.callCount).to.eq(0);
                    expect(del.callCount).to.eq(0);
                });

        });
    });

    describe('getCollectionInNamedCollection', () => {

        xit('should not need update when the same ', () => {

            const get = sinon.stub();
            const post = sinon.stub();
            const put = sinon.stub();
            const del = sinon.stub();

            const document = {};

            const sparseCollection = cache.makeSparseCollectionResourceFromUri('https://api.example.com/tenant');

            return sync.getCollectionInNamedCollection(sparseCollection, document, [], {
                getFactory: get,
                postFactory: post,
                putFactory: put,
                deleteFactory: del
            })
                .then(result => {
                    expect(result).to.not.be.undefined;
                    expect(get.callCount).to.eq(2);
                    expect(put.callCount).to.eq(1);
                    expect(post.callCount).to.eq(0);
                    expect(del.callCount).to.eq(0);
                });

        });

    });

    describe('getResourceInNamedCollection', () => {
        // TODO
    });

    describe('getResourceInCollection', () => {

        it('should sync a document into a collection', () => {

            const document = {
                name: 'Rewire NZ (copy)',
                code: 'copy.rewire.example.nz',
                description: 'A sample tenant (company/organisation)',
            };
            const collection = {
                links: [
                    {
                        rel: 'self',
                        href: 'https://api.example.com/tenant'
                    },
                    {
                        rel: 'create-form',
                        href: 'https://api.example.com/tenant/form/create'
                    },
                ],
                items: []
            };
            const createForm = {
                links: [
                    {
                        rel: 'self',
                        href: 'https://api.example.com/tenant/form/create'
                    },
                    {
                        rel: 'submit',
                        href: 'https://api.example.com/tenant'
                    }
                ],
                items: [
                    {
                        type: 'http://types/text',
                        name: 'code',
                        required: true,
                        description: 'The unique name in domain name format'
                    },
                    {
                        type: 'http://types/text',
                        name: 'name',
                        description: 'The name of the tenant to be shown on the screen'
                    },
                    {
                        type: 'http://types/text',
                        name: 'description',
                        description: 'Other details about the organisation'
                    }
                ]
            };

            const get = sinon.stub();
            const post = sinon.stub();

            get.onCall(0).callsFake(() => {
                log.info('[Test] GET collection');
                return Promise.resolve({data: collection});
            });
            get.onCall(1).callsFake(() => {
                log.info('[Test] GET create form');
                return Promise.resolve({data: createForm});
            });

            post.onFirstCall().callsFake(() => {
                log.info('[Test] POST document');
                return Promise.resolve({headers: {location: 'https://api.example.com/tenant/XXXX'}});
            });

            get.onCall(2).callsFake(() => {
                log.info('[Test] GET created document');
                return Promise.resolve({data: document});
            });


            const coll = cache.makeSparseCollectionResourceFromUri('https://api.example.com/tenant');

            return sync.getResourceInCollection(coll, document, [], {getFactory: get, postFactory: post})
                .then(result => {
                    expect(result).to.not.be.undefined;
                    expect(get.callCount).to.eq(3);
                    expect(post.callCount).to.eq(1);
                });

        });


    });

});