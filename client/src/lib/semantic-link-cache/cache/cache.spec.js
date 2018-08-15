import {expect} from 'chai';
import * as cache from './cache';
import sinon from 'sinon';
import State from './State';

import {stateFlagEnum} from './stateFlagEnum';

global.Element = () => {
};


/**
 * Increasing counter to create new & unique resource names
 * @type {number}
 */
let repCount = 1;

/**
 * Return an url based on the incremented counter to make href unique
 * @param {string=} prefix=http://example.com/
 * @return {string} of url type
 */
const uniqueUri = prefix => prefix || 'http://example.com/' + (++repCount);

describe('Cache', () => {
    let linkedRepresentation, feedRepresentation;

    describe('Synch: cache simple functions - synchronise', () => {

        describe('Unknown state', () => {
            beforeEach(() => {
                linkedRepresentation = cache.makeSparseResourceFromUri(uniqueUri());
            });

            describe('makeUnknownResourceAddedToResource', () => {

                it('Does not add a tracked resource into state when attribute of same name exists', () => {
                    linkedRepresentation.name = 'N';
                    const result = cache.makeUnknownResourceAddedToResource(linkedRepresentation, 'name');

                    expect(result).to.equal('N');
                    expect(State.get(linkedRepresentation).isTracked('name')).to.be.false;
                });

                it('Does add a tracked resource into state when attribute of same name does not exist', () => {
                    const result = cache.makeUnknownResourceAddedToResource(linkedRepresentation, 'name');

                    expect(result).to.not.be.null;
                    expect(State.get(linkedRepresentation).isTracked('name')).to.be.true;
                    expect(State.get(result).getStatus()).to.equal(stateFlagEnum.unknown);
                });

                it('Makes a new one with defaults', () => {
                    const result = cache.makeUnknownResourceAddedToResource(linkedRepresentation, 'name', {
                        attr1: 'value'
                    });
                    expect(result.attr1).to.equal('value');
                });

                it('has a set of links that are properly formed', () => {
                    expect(linkedRepresentation.links).to.be.an('array');
                    expect(linkedRepresentation.links[0].rel).to.be.a('string');
                    expect(linkedRepresentation.links[0].href).to.be.a('string');
                });
            });

            describe('makeUnknownCollectionAddedToResource', () => {
                it('Does not add a tracked resource into state when attribute of same name exists', () => {
                    const feed = cache.makeSparseResourceFromUri(uniqueUri());
                    feed.items = [
                        {id: uniqueUri(), title: 'a'},
                        {id: uniqueUri(), title: 'b'},
                    ];
                    linkedRepresentation.name = feed;

                    const result = cache.makeUnknownCollectionAddedToResource(linkedRepresentation, 'name');

                    expect(result).to.equal(feed);
                    expect(State.get(linkedRepresentation).isTracked('name')).to.be.false;

                });

                it('Does add a tracked resource into state when attribute of same name does not exist', () => {
                    const result = cache.makeUnknownCollectionAddedToResource(linkedRepresentation, 'name');

                    expect(result).to.not.be.null;
                    expect(State.get(linkedRepresentation).isTracked('name')).to.be.true;
                    expect(State.get(result).getStatus()).to.equal(stateFlagEnum.unknown);
                });

                it('Makes a new one with defaults including empty items attribute', () => {
                    const result = cache.makeUnknownCollectionAddedToResource(linkedRepresentation, 'name', {
                        attr1: 'value'
                    });
                    expect(result.attr1).to.equal('value');
                    expect(result.items).to.not.be.null;
                    expect(result.items.length).to.equal(0);
                });

                it('Makes a new one with defaults with items provided', () => {
                    const defaultValues = {
                        attr1: 'value',
                        items: [{
                            id: 'http://example.com',
                            title: 'bla'
                        }]
                    };
                    const result = cache.makeUnknownCollectionAddedToResource(linkedRepresentation, 'name', defaultValues);
                    expect(result.attr1).to.equal('value');
                    expect(result.items.length).to.equal(1);
                    expect(result.items[0].id).to.equal('http://example.com');
                });

            });

            describe('makeUnknownResourceAddedToCollection', () => {
                beforeEach(() => {
                    feedRepresentation = cache.makeSparseCollectionResourceFromUri(uniqueUri());
                });
                it('Does add a new item to the collection', () => {
                    const result = cache.makeUnknownResourceAddedToCollection(feedRepresentation);

                    expect(result).to.not.be.null;
                    expect(feedRepresentation.items).to.deep.equal([result]);
                });

                it('Makes a new one with defaults', () => {
                    const result = cache.makeUnknownResourceAddedToCollection(feedRepresentation, {
                        extra: 3
                    });
                    expect(result.extra).to.equal(3);
                    expect(feedRepresentation.items).to.deep.equal([result]);
                });
            });

        });

        describe('LocationOnly state', () => {

            describe('addCollectionResourceItemByUri', () => {
                beforeEach(() => {
                    feedRepresentation = cache.makeSparseCollectionResourceFromUri(uniqueUri());
                });

                it('add', () => {
                    const uri = uniqueUri();
                    const result = cache.addCollectionResourceItemByUri(feedRepresentation, uri);

                    expect(result.links).to.deep.equal([{rel: 'self', href: uri}]);
                    expect(feedRepresentation.items).to.deep.equal([result]);
                    expect(State.get(result).getStatus()).not.to.equal(stateFlagEnum.hydrated);
                    expect(State.get(result).getStatus()).to.equal(stateFlagEnum.locationOnly);
                });

                it('add with defaults', () => {
                    const defaultLinked = {
                        extra: 3
                    };
                    const result = cache.addCollectionResourceItemByUri(feedRepresentation, uniqueUri(), defaultLinked);

                    expect(result.extra).to.equal(3);
                    expect(result.items).to.be.undefined;
                });
            });
        });

    });

    describe('Async: cache get/add resources - async promised-based', () => {

        describe('getResource', () => {

            it('retrieves the resource', () => {

                let stateFactory = sinon.stub(State, 'get')
                    .returns({
                        getResource: () => Promise.resolve({x: 4, links: []})
                    });

                return cache.getResource({})
                    .then(newResource => {
                        expect(newResource.x).to.equal(4);
                        expect(newResource.links).to.not.be.null;
                        expect(stateFactory.called).to.be.true;
                        stateFactory.restore();

                    });

            });
        });

        describe('tryGetResource', () => {

            it('retrieves the resource when value', () => {

                let stateFactory = sinon.stub(State, 'tryGet')
                    .returns({
                        getResource: () => Promise.resolve({x: 4, links: []})
                    });

                return cache.tryGetResource({})
                    .then(newResource => {
                        expect(newResource.x).to.equal(4);
                        expect(newResource.links).to.not.be.null;
                        expect(stateFactory.called).to.be.true;
                        stateFactory.restore();

                    });
            });

            it('returns default value when undefined', () => {

                let stateFactory = sinon.stub(State, 'tryGet')
                    .returns(undefined);

                return cache.tryGetResource({}, undefined)
                    .then(newResource => {
                        expect(newResource).not.to.not.be.null;
                        expect(stateFactory.called).to.be.true;
                        stateFactory.restore();

                    });
            });

        });

    });
});