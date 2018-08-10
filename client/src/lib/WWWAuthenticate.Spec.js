import * as wwwAuthenticate from './WWWAuthenticate';
import {expect} from 'chai';

describe('Has Loaded', function () {
    it('Util: wwwAuthenticate', function () {
        expect(wwwAuthenticate).to.not.be.undefined;
    });
});

describe('Parser', function () {

    let header = 'Negotiate, Basic realm="example.com", Resource uri=https://example.com/authenticator, Bearer realm="example.com"';

    it('should return uri as default', function () {
        expect(wwwAuthenticate.matchNegotiateType(header)).to.equal('https://example.com/authenticator');
    });
    it('should return uri when parameterised', function () {
        expect(wwwAuthenticate.matchNegotiateType(header, 'Resource')).to.equal('https://example.com/authenticator');
    });

});