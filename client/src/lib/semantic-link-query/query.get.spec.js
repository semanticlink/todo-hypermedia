import {expect} from 'chai';
import * as query from './query';
import sinon from 'sinon';
import * as cache from 'semantic-link-cache/cache/cache';
// make sure that you stub the js implementation and not the type declaration
import * as http from 'semantic-link/lib/http';

global.Element = () => {
};

const stub = methodName => sinon.stub(cache, methodName)
    .returns(Promise.resolve());

const stubGet = result => sinon.stub(http, http.get.name).returns(Promise.resolve({data: result}));

describe('Cache', () => {

    let resourceFactory;

    const singleton = {};
    const collection = {items: []};

    it(cache.getResource.name, () => {
        resourceFactory = stub(cache.getResource.name);
        return query.get(singleton);
    });

    it(cache.tryGetResource.name, () => {
        resourceFactory = stub(cache.tryGetResource.name);
        return query.get(singleton, {defaultRepresentation: {}});
    });

    it(cache.getCollection.name, () => {
        resourceFactory = stub(cache.getCollection.name);
        return query.get(collection);
    });

    it(cache.getCollectionItem.name, () => {
        resourceFactory = stub(cache.getCollectionItem.name);
        return query.get(collection, {where: {links: [{rel: 'self', href: 'https://api.example.com/item/1'}]}});
    });

    it(cache.getCollectionItemByUri.name, () => {
        resourceFactory = stub(cache.getCollectionItemByUri.name);
        return query.get(collection, {where: 'https://api.example.com/item/1'});
    });

    describe('Named resources', () => {

        let getFactory;

        describe('Singleton', () => {

            beforeEach(() => {
                getFactory = stubGet(singleton);
            });

            it(`${cache.getSingleton.name} on resource`, () => {
                resourceFactory = stub(cache.getSingleton.name);
                return query.get(singleton, {rel: 'tags'});
            });

            it(`${cache.getSingleton.name} on collection`, () => {
                resourceFactory = stub(cache.getSingleton.name);
                return query.get(collection, {rel: 'tags'});
            });

            it(`${cache.tryGetSingleton.name} on resource`, () => {
                resourceFactory = stub(cache.tryGetSingleton.name);
                return query.get(singleton, {rel: 'tags', defaultRepresentation: {}});
            });


        });

        describe('Collection', () => {

            beforeEach(() => {
                getFactory = stubGet(collection);
            });

            it(`${cache.getNamedCollection.name} on singleton`, () => {
                resourceFactory = stub(cache.getNamedCollection.name);
                return query.get(singleton, {rel: 'tags'});
            });

            it(`${cache.getNamedCollection.name} on collection`, () => {
                resourceFactory = stub(cache.getNamedCollection.name);
                return query.get(collection, {rel: 'tags'});
            });

            it(cache.getNamedCollectionAndItems.name, () => {
                resourceFactory = stub(cache.getNamedCollectionAndItems.name);
                return query.get(singleton, {rel: 'tags', include: 'items'});
            });

            it(cache.tryGetNamedCollectionAndItemsOnCollectionItems.name, () => {
                resourceFactory = stub(cache.tryGetNamedCollectionAndItemsOnCollectionItems.name);
                return query.get(collection, {rel: 'tags', include: 'items'});
            });

            it(cache.getNamedCollectionItemByUri.name, () => {
                resourceFactory = stub(cache.getNamedCollectionItemByUri.name);
                return query.get(singleton, {rel: 'tags', where: 'https://api.example.com/item/1'});
            });

        });

        afterEach((() => {
            expect(getFactory.called).to.be.true;
            getFactory.restore();
        }));

    });

    it(cache.getNamedCollectionOnSingletons.name, () => {
        resourceFactory = stub(cache.getNamedCollectionOnSingletons.name);
        return query.get([singleton, singleton], {rel: 'tags'});
    });


    afterEach((() => {
        expect(resourceFactory.called).to.be.true;
        resourceFactory.restore();
    }));

});
