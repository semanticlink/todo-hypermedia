import * as _ from './uri-list';
import {expect} from 'chai';


describe('_.makeUriList', () => {

    it('should keep string as uri', () => {
        expect(_.mapResourceToUriList('http://example.com/role/1')).to.deep.equal(['http://example.com/role/1']);
    });
    it('should keep an array string as uri', () => {
        const uriList = ['http://example.com/role/1', 'http://example.com/role/2'];
        expect(_.mapResourceToUriList(uriList)).to.deep.equal(uriList);
    });
    it('should transform representation to string', () => {
        const resource = {
            links: [{
                rel: 'self',
                href: 'http://example.com/role/1'
            }],
            name: 'Administrator'
        };
        expect(_.mapResourceToUriList(resource)).to.deep.equal(['http://example.com/role/1']);
    });
});

describe('_.mapUriList', () => {

    it('should convert items', () => {
        const values = [
            {links: [{rel: 'self', href: 'http://localhost:1080/role/50'}],},
            {links: [{rel: 'self', href: 'http://localhost:1080/role/49'}],}
        ];
        expect(_.mapCollectionItemsToUriList(values)).to.deep.equal(['http://localhost:1080/role/50', 'http://localhost:1080/role/49']);
    });

    it('should not return undefined in the list', () => {
        const values = [
            {links: [{rel: 'self', href: 'http://localhost:1080/role/50'}],},
            {links: [{rel: 'up', href: 'http://localhost:1080/role/49'}],}
        ];
        expect(_.mapCollectionItemsToUriList(values)).to.deep.equal(['http://localhost:1080/role/50']);
    });

    it('should empty list on empty list', () => {
        const values = [];
        expect(_.mapCollectionItemsToUriList(values)).to.deep.equal([]);
    });

    it('should empty list on empty list on none found', () => {
        const values = [
            {links: [{rel: 'up', href: 'http://localhost:1080/role/49'}],}
        ];
        expect(_.mapCollectionItemsToUriList(values)).to.deep.equal([]);
    });

    it('should empty list on empty list on undefined', () => {
        expect(_.mapCollectionItemsToUriList(undefined)).to.deep.equal([]);
    });

});

