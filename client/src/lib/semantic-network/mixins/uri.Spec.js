import * as _ from './uri';
import {expect} from 'chai';


describe('_.makeUri', () => {

    it('should keep string as uri', () => {
        expect(_.makeUri('http://example.com/role/1')).to.equal('http://example.com/role/1');
    });
    it('should transform representation to string', () => {
        const resource = {
            links: [{
                rel: 'self',
                href: 'http://example.com/role/1'
            }]
        };
        expect(_.makeUri(resource)).to.equal('http://example.com/role/1');
    });
});

