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
            const put = sinon.stub();
            const del = sinon.stub();

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

            return sync.getResourceInCollection(coll, document, [], {
                getFactory: get,
                postFactory: post,
                putFactory: put,
                deleteFactory: del
            })
                .then(result => {
                    expect(result).to.not.be.undefined;
                    expect(get.callCount).to.eq(3);
                    expect(post.callCount).to.eq(1);
                    expect(put.callCount).to.eq(0);
                    expect(del.callCount).to.eq(0);
                });

        });

    });


    describe('getResourceInNamedCollection', () => {

        const parent = {
            links: [
                {
                    rel: 'self',
                    href: 'https://api.example.com/user/f58c6dd2a5'
                },
                {
                    rel: 'todos',
                    href: 'https://api.example.com/user/tenant/90a936d4a3/todo'
                },
            ],
            name: 'test',
            email: 'test@rewire.nz'
        };
        const namedCollection = {
            links: [
                {
                    rel: 'self',
                    href: 'https://api.example.com/user/tenant/90a936d4a3/todo'
                },
                {
                    rel: 'create-form',
                    href: 'https://api.example.com/todo/form/create'
                }
            ],
            items: [
                {
                    id: 'https://api.example.com/todo/4bf1d09bb0',
                    title: 'One Todo'
                },
                {
                    id: 'https://api.example.com/todo/a3e0ce2e0d',
                    title: 'Two Todo (tag)'
                }
            ]
        };
        const editForm = {
            links: [
                {
                    rel: 'self',
                    href: 'https://api.example.com/todo/form/edit'
                }
            ],
            items: [
                {
                    type: 'http://types/text',
                    name: 'name',
                    required: true,
                    description: 'The title of the page'
                },
                {
                    type: 'http://types/select',
                    name: 'state',
                    description: 'A todo can only toggle between open and complete.',
                    items: [
                        {
                            type: 'http://types/enum',
                            value: 'http://example.com/todo/state/complete',
                            label: 'Completed',
                            name: 'completed',
                            description: 'The todo has been completed'
                        },
                        {
                            type: 'http://types/enum',
                            value: 'http://example.com/todo/state/open',
                            label: 'Open',
                            name: 'open',
                            description: 'The todo has been opened'
                        }
                    ]
                },
                {
                    type: 'http://types/datetime',
                    name: 'due',
                    description: 'The UTC date the todo is due'
                }
            ]
        };
        const createForm = {
            links: [
                {
                    rel: 'self',
                    href: 'https://api.example.com/todo/form/create'
                }
            ],
            items: [
                {
                    type: 'http://types/text',
                    name: 'name',
                    required: true,
                    description: 'The title of the page'
                },
                {
                    type: 'http://types/select',
                    name: 'state',
                    description: 'A todo can only toggle between open and complete.',
                    items: [
                        {
                            type: 'http://types/enum',
                            value: 'http://example.com/todo/state/complete',
                            label: 'Completed',
                            name: 'completed',
                            description: 'The todo has been completed'
                        },
                        {
                            type: 'http://types/enum',
                            value: 'http://example.com/todo/state/open',
                            label: 'Open',
                            name: 'open',
                            description: 'The todo has been opened'
                        }
                    ]
                },
                {
                    type: 'http://types/datetime',
                    name: 'due',
                    description: 'The UTC date the todo is due'
                }
            ]
        };

        let get;
        let post;
        let put;
        let del;
        let options;

        beforeEach(() => {
            get = sinon.stub();
            post = sinon.stub();
            put = sinon.stub();
            del = sinon.stub();

            options = {
                getFactory: get,
                postFactory: post,
                putFactory: put,
                deleteFactory: del
            };
        });

        it('should not update when the document is the same', () => {

            const matchedItem = {
                links: [
                    {
                        rel: 'self',
                        href: 'https://api.example.com/todo/a3e0ce2e0d'
                    },
                    {
                        rel: 'edit-form',
                        href: 'https://api.example.com/todo/form/edit'
                    }
                ],
                name: 'Two Todo (tag)',
                state: 'http://example.com/todo/state/complete',
                due: '0001-01-01T11:40:00+11:40'
            };
            const noChangeDocument = {
                links: [
                    {
                        rel: 'self',
                        href: 'https://api.example.com/todo/a3e0ce2e0d'
                    },
                    {
                        rel: 'edit-form',
                        href: 'https://api.example.com/todo/form/edit'
                    }
                ],
                name: 'Two Todo (tag)',
                state: 'http://example.com/todo/state/complete',
                due: '0001-01-01T11:40:00+11:40'
            };

            get.onCall(0).callsFake(() => {
                log.info('[Test] GET named collection');
                return Promise.resolve({data: namedCollection});
            });
            get.onCall(1).callsFake(() => {
                log.info('[Test] GET item from collection');
                return Promise.resolve({data: matchedItem});
            });

            get.onCall(2).callsFake(() => {
                log.info('[Test] GET edit form');
                return Promise.resolve({data: editForm});
            });

            put.onFirstCall().callsFake(() => {
                log.info('[Test] PUT document');
                return Promise.resolve({});
            });

            const sparseParent = cache.makeSparseCollectionResourceFromUri('https://api.example.com/user/f58c6dd2a5', parent);

            return sync.getResourceInNamedCollection(sparseParent, 'todos', /todos/, noChangeDocument, [], options)
                .then(result => {
                    expect(result).to.not.be.undefined;
                    expect(get.callCount).to.eq(3);
                    expect(put.callCount).to.eq(0);
                    expect(post.callCount).to.eq(0);
                    expect(del.callCount).to.eq(0);
                });

        });

        it('should update when the document matches an item and an attribute is different', () => {

            const matchedItem = {
                links: [
                    {
                        rel: 'self',
                        href: 'https://api.example.com/todo/a3e0ce2e0d'
                    },
                    {
                        rel: 'edit-form',
                        href: 'https://api.example.com/todo/form/edit'
                    }
                ],
                name: 'Two Todo (tag)',
                state: 'http://example.com/todo/state/complete',
                due: '0001-01-01T11:40:00+11:40'
            };
            const changedDocument = {
                links: [
                    {
                        rel: 'self',
                        href: 'https://api.example.com/todo/a3e0ce2e0d'
                    },
                    {
                        rel: 'edit-form',
                        href: 'https://api.example.com/todo/form/edit'
                    }
                ],
                name: 'Two Todo (tag) [Updated]',
                state: 'http://example.com/todo/state/complete',
                due: '0001-01-01T11:40:00+11:40'
            };

            get.onCall(0).callsFake(() => {
                log.info('[Test] GET named collection');
                return Promise.resolve({data: namedCollection});
            });
            get.onCall(1).callsFake(() => {
                log.info('[Test] GET item from collection');
                return Promise.resolve({data: matchedItem});
            });

            get.onCall(2).callsFake(() => {
                log.info('[Test] GET edit form');
                return Promise.resolve({data: editForm});
            });

            put.onFirstCall().callsFake(() => {
                log.info('[Test] PUT document');
                return Promise.resolve({});
            });

            const sparseParent = cache.makeSparseCollectionResourceFromUri('https://api.example.com/user/f58c6dd2a5', parent);

            return sync.getResourceInNamedCollection(sparseParent, 'todos', /todos/, changedDocument, [], options)
                .then(result => {
                    expect(result).to.not.be.undefined;
                    expect(get.callCount).to.eq(3);
                    expect(put.callCount).to.eq(1);
                    expect(post.callCount).to.eq(0);
                    expect(del.callCount).to.eq(0);
                });

        });

        it('should add when the document is not found in collection', () => {

            const newDocument = {
                name: 'Brand new',
                state: 'http://example.com/todo/state/complete',
                due: '0001-01-01T11:40:00+11:40'
            };

            get.onCall(0).callsFake(() => {
                log.info('[Test] GET named collection');
                return Promise.resolve({data: namedCollection});
            });

            get.onCall(1).callsFake(() => {
                log.info('[Test] GET create form');
                return Promise.resolve({data: createForm});
            });

            post.onCall(0).callsFake(() => {
                log.info('[Test] PUT document');
                return Promise.resolve({headers: {location: 'https://api.example.com/todo/new'}});
            });

            get.onCall(2).callsFake(() => {
                log.info('[Test] GET edit form');
                return Promise.resolve({data: createForm});
            });

            const sparseParent = cache.makeSparseCollectionResourceFromUri('https://api.example.com/user/f58c6dd2a5', parent);

            return sync.getResourceInNamedCollection(sparseParent, 'todos', /todos/, newDocument, [], options)
                .then(result => {
                    expect(result).to.not.be.undefined;
                    expect(get.callCount).to.eq(3);
                    expect(put.callCount).to.eq(0);
                    expect(post.callCount).to.eq(1);
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

    describe('getNamedCollectionInNamedCollection', () => {
        // TODO
    });

    describe('getSingleton', () => {
        // TODO
    });

});