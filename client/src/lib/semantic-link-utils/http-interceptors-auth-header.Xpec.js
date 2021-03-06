import {expect} from 'chai';
import parse from 'auth-header/src/parse';
import format from 'auth-header/src/format';

describe('Authorization headers', () => {
    describe('www-authenticate', () => {

        it('should handle Bearer', () => {
            const result = parse('Bearer mF_9.B5f-4.1JqM');
            expect(result).to.deep.equal({
                scheme: 'Bearer',
                token: 'mF_9.B5f-4.1JqM',
                params: {},
            });
        });

        it('should handle Bearer with realm, rel and url', () => {
            const result = parse('Bearer realm="api", rel="authenticate", uri="http://example.com/"');
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
            const result = parse('Bearer realm="api", rel=authenticate, uri=http://example.com/');
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
            const result = parse('Bearer realm="api", rel=authenticate, uri=http://example.com/ realm="facebook", rel=authenticate, uri=http://facebook.com/');
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
            const result = parse('Bearer realm="api", rel=authenticate, uri=http://example.com/ realm="facebook"');
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
            const result = parse('Bearer realm="api", rel=authenticate, uri=http://example.com/ mF_9.B5f-4.1JqM');
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
            const res = format({
                scheme: 'Bearer',
                token: 'QWxhZGRpbjpvcGVuIHNlc2FtZQ==',
            });
            expect(res).to.equal('Bearer QWxhZGRpbjpvcGVuIHNlc2FtZQ==');
        });

    });
});
