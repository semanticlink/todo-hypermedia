import {expect} from 'chai';
import sinon from 'sinon';
import * as link from 'semantic-link';
import * as sync from './sync';
import * as cache from '../cache/cache';
import Differencer from './Differencer';
import axios from 'axios';
import {log} from 'semantic-link/lib/logger';

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

        it('should not need update when the same ', () => {

            const get = sinon.stub();
            const post = sinon.stub();
            const put = sinon.stub();
            const del = sinon.stub();

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

    xdescribe('getUriListOnNamedCollection', () => {


        it('should return undefined on no singleton rel found', () => {
            const getResource = sinon.stub(cache, 'getResource');
            const tryGetNamedCollectionResource = sinon.stub(cache, 'tryGetNamedCollectionResource');

            let uriList = ['http://example.com/question/item/1'];
            let resource = {
                links: [{rel: 'notifications', href: ''}]
            };

            getResource.returns(Promise.resolve(resource));
            tryGetNamedCollectionResource.returns(Promise.resolve(undefined));

            return sync.getUriListOnNamedCollection(resource, 'notifications', /notifications/, uriList, {})
                .then(result => {
                    expect(result).to.be.undefined;
                });
        });

        it('should be able to add a single url', () => {
            const getResource = sinon.stub(cache, 'getResource');
            const tryGetNamedCollectionResource = sinon.stub(cache, 'tryGetNamedCollectionResource');
            const getCollectionResourceAndItems = sinon.stub(cache, 'getCollectionResourceAndItems');


            let uriList = ['http://example.com/question/item/1'];

            let item = {

                'links': [
                    {
                        'rel': 'self',
                        'href': 'http://api.example.com/survey/question/notification/1504'
                    },
                    {
                        'rel': 'up',
                        'href': 'http://api.example.com/question/107214'
                    },
                    {
                        'rel': 'question-item',
                        'href': 'http://api.example.com/question/item/705233'
                    }
                ],
                'email': [
                    'http://api.example.com/user/02f46ed8-3b6e-4cef-ae47-e15cccf730eb'
                ]

            };

            let resource = {

                'links': [
                    {
                        'rel': 'self',
                        'href': 'http://api.example.com/user/02f46ed8-3b6e-4cef-ae47-e15cccf730eb/notification/'
                    }
                ],
                'items': [item]

            };

            let options = {
                uriListResolver: () => 'http://example.com/question/item/1'
            };

            getResource.returns(Promise.resolve(undefined));
            tryGetNamedCollectionResource.returns(Promise.resolve(resource));
            getCollectionResourceAndItems.returns(Promise.resolve(resource));

            const diffUriList = sinon.stub(Differencer, 'diffUriList')
                .returns(Promise.resolve([]));

            return sync.getUriListOnNamedCollection(resource, 'notifications', /notifications/, uriList, options)
                .then(result => {
                    expect(getResource.called).to.be.true;
                    expect(tryGetNamedCollectionResource.called).to.be.true;
                    expect(diffUriList.called).to.be.true;
                    expect(getCollectionResourceAndItems.called).to.be.true;
                    expect(result).to.equal(resource);
                    Differencer.diffUriList.restore();

                });
        });

        describe('resolvers', () => {

            beforeEach(() => {

                const getResource = sinon.stub(cache, 'getResource');
                const tryGetNamedCollectionResource = sinon.stub(cache, 'tryGetNamedCollectionResource');
                const getCollectionResourceAndItems = sinon.stub(cache, 'getCollectionResourceAndItems');

                getResource.returns(Promise.resolve());
                tryGetNamedCollectionResource.returns(Promise.resolve({singleton: ''}));
                getCollectionResourceAndItems.returns(Promise.resolve());

            });

            it('should use default create resolver POSTs back and add to resolver', () => {

                let post = sinon.stub(link, 'post').returns(Promise.resolve({
                    status: 201,
                    headers: () => {
                        return {
                            location: ''
                        };
                    }
                }));

                let get = sinon.stub(axios, 'get')
                    .returns(Promise.resolve({
                        url: 'http://example.com/notification/',
                        status: 200,
                        data: {items: [{id: 'http://example.com/notification/1'}]},
                    }));

                const diffUriList = sinon.stub(Differencer, 'diffUriList')
                    .callsFake((x, y, options) => Promise.resolve(options.createStrategy([])));

                let options = {
                    resolver: {
                        add: sinon.stub(),
                    }
                };

                return sync.getUriListOnNamedCollection([], 'notifications', /notifications/, [], options)
                    .then(() => {
                        expect(options.resolver.add.called).to.be.true;
                        expect(post.called).to.be.true;
                        expect(get.called).to.be.true;
                        expect(diffUriList.called).to.be.true;
                    })
                    .catch()
                    .then(() => {
                        link.post.restore();
                        axios.get.restore();
                        Differencer.diffUriList.restore();
                    });
            });

            it('should use default delete resolver that DELETEs and removes Uri to resolver', () => {

                let del = sinon.stub(link, 'delete').returns(Promise.resolve({
                    status: 200
                }));

                const diffUriList = sinon.stub(Differencer, 'diffUriList')
                    .callsFake((x, y, options) => Promise.resolve(options.deleteStrategy(['item'])));

                let options = {
                    readonly: false,
                    resolver: {
                        remove: sinon.stub()
                    }
                };

                return sync.getUriListOnNamedCollection([], 'notifications', /notifications/, [], options)
                    .then(() => {
                        sinon.assert.callOrder(diffUriList, del, options.resolver.remove);
                        link.delete.restore();
                        Differencer.diffUriList.restore();

                    });
            });

            it('should correctly construct text/url-list mime type format', () => {

                let del = sinon.stub(link, 'delete')
                    .callsFake(
                        (/*resource, rel, mimeType, data */) => {
                            //expect(data).to.equal('item\r\nitem2\r\n');
                            return Promise.resolve({
                                status: 200
                            });
                        });

                sinon.stub(Differencer, 'diffUriList')
                    .callsFake((x, y, options) => Promise.resolve(options.deleteStrategy(['item', 'item2'])));

                let options = {
                    resolver: {
                        remove: sinon.stub()
                    }
                };

                return sync.getUriListOnNamedCollection({link: []}, 'notifications', /notifications/, [], options)
                    .then(() => {
                        expect(del.called).to.be.true;
                        Differencer.diffUriList.restore();
                        link.delete.restore();

                    });
            });

            describe('change the singleton resource', () => {

                const options = {
                    resolver: {
                        remove: sinon.stub(),
                        add: sinon.stub(),
                    }
                };

                it('should call resolver with singleton on POST', () => {

                    let post = sinon.stub(link, 'post')
                        .callsFake(resource => {
                            expect(resource).to.deep.equal({singleton: ''});
                            return Promise.resolve({
                                status: 201,
                                headers: () => {
                                    return {
                                        location: ''
                                    };
                                }
                            });
                        });

                    let get = sinon.stub(axios, 'get')
                        .returns(Promise.resolve({
                            url: 'http://example.com/notification/',
                            status: 200,
                            data: {items: [{id: 'http://example.com/notification/1'}]},
                        }));

                    const diffUriList = sinon.stub(Differencer, 'diffUriList')
                        .callsFake((x, y, options) => Promise.resolve(options.createStrategy([])));

                    return sync.getUriListOnNamedCollection([], 'notifications', /notifications/, [], options)
                        .then(() => {
                            expect(options.resolver.add.called).to.be.true;
                            expect(post.called).to.be.true;
                            expect(get.called).to.be.true;
                            expect(diffUriList.called).to.be.true;

                            Differencer.diffUriList.restore();
                            link.post.restore();
                            axios.get.restore();

                        });
                });

                it('should call resolver with singleton on DELETE', () => {

                    let del = sinon.stub(link, 'delete')
                        .callsFake(resource => {
                            expect(resource).to.deep.equal({singleton: ''});
                            return Promise.resolve({
                                status: 200
                            });
                        });

                    sinon.stub(Differencer, 'diffUriList')
                        .callsFake((x, y, options) => Promise.resolve(options.deleteStrategy(['item', 'item2'])));

                    return sync.getUriListOnNamedCollection({link: []}, 'notifications', /notifications/, [], options)
                        .then(() => {
                            expect(del.called).to.be.true;

                            link.delete.restore();
                            Differencer.diffUriList.restore();

                        });
                });
            });

        });

    });

})
;