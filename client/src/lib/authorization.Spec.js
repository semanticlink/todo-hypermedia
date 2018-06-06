import { expect } from 'chai';
import * as authorization from 'auth-header';

describe('Authorization headers', () => {
    describe('www-authenticate', () => {

        it('should handle Bearer', () => {
            const result = authorization.parse('Bearer mF_9.B5f-4.1JqM');
            expect(result).to.deep.equal({
                scheme: 'Bearer',
                token: 'mF_9.B5f-4.1JqM',
                params: {},
            });
        });

        it('should handle Bearer with realm, rel and url', () => {
            const result = authorization.parse('Bearer realm="api", rel="authenticate", uri="http://example.com/"');
            expect(result).to.deep.equal({
                scheme: 'Bearer',
                token: null,
                params: {
                    uri: 'http://example.com/',
                    realm: 'api',
                    rel: 'authenticate'
                },
            });
        });

        it('should handle Bearer with ream, rel and uri with optional quotes', () => {
            const result = authorization.parse('Bearer realm="api", rel=authenticate, uri=http://example.com/');
            expect(result).to.deep.equal({
                scheme: 'Bearer',
                token: null,
                params: {
                    uri: 'http://example.com/',
                    realm: 'api',
                    rel: 'authenticate'
                },
            });
        });

        it('should handle multiple entries per line', () => {
            const result = authorization.parse('Bearer realm="api", rel=authenticate, uri=http://example.com/ realm="facebook", rel=authenticate, uri=http://facebook.com/');
            expect(result).to.deep.equal({
                scheme: 'Bearer',
                token: null,
                params: {
                    uri: ['http://example.com/', 'http://facebook.com/'],
                    realm: ['api', 'facebook'],
                    rel: ['authenticate', 'authenticate']
                },
            });
        });

        it('should handle multiple entries per line that have different keys', () => {
            const result = authorization.parse('Bearer realm="api", rel=authenticate, uri=http://example.com/ realm="facebook"');
            expect(result).to.deep.equal({
                scheme: 'Bearer',
                token: null,
                params: {
                    uri: 'http://example.com/',
                    realm: ['api', 'facebook'],
                    rel: 'authenticate'
                },
            });
        });

        it('should handle multiple entries, realm and token', () => {
            const result = authorization.parse('Bearer realm="api", rel=authenticate, uri=http://example.com/ mF_9.B5f-4.1JqM');
            expect(result).to.deep.equal({
                scheme: 'Bearer',
                token: 'mF_9.B5f-4.1JqM',
                params: {
                    uri: 'http://example.com/',
                    realm: 'api',
                    rel: 'authenticate'
                },
            });
        });


    });
    describe('authorization', () => {
        it('should produce correct token property', () => {
            const res = authorization.format({
                scheme: 'Bearer',
                token: 'QWxhZGRpbjpvcGVuIHNlc2FtZQ==',
            });
            expect(res).to.equal('Bearer QWxhZGRpbjpvcGVuIHNlc2FtZQ==');
        });

    });
});
