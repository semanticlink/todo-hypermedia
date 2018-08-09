import * as _ from './uri-list';
import {expect} from 'chai';


describe('_.makeUriList', () => {

    it('should keep string as uri', () => {
        expect(_.makeUriList('http://example.com/role/1')).to.deep.equal(['http://example.com/role/1']);
    });
    it('should keep an array string as uri', () => {
        const uriList = ['http://example.com/role/1', 'http://example.com/role/2'];
        expect(_.makeUriList(uriList)).to.deep.equal(uriList);
    });
    it('should transform representation to string', () => {
        const resource = {
            links: [{
                rel: 'self',
                href: 'http://example.com/role/1'
            }],
            name: 'Administrator'
        };
        expect(_.makeUriList(resource)).to.deep.equal(['http://example.com/role/1']);
    });
});
