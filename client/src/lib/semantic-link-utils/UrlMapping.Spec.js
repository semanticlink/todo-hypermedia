/*eslint-env mocha */
import UriMapping, { fromSitePath, toSitePath } from './UriMapping';
import { expect } from 'chai';

/**
 * @type {UriMapping}
 */
let mapping = new UriMapping('', '');

describe('Has Loaded', function () {
    it('Uri Mapping', function () {
        expect(UriMapping).to.not.be.undefined;
    });
});

describe('API and client app with trailing slash', function () {

    const clientUri = 'https://client.example.com/';
    const apiUri = 'https://api.example.com/';

    beforeEach(() => {
        mapping.initialise(clientUri, apiUri);
    });

    describe('Root', function () {

        it('Api URI to site path', function () {
            expect(toSitePath('', '')).to.equal('');
        });

        it('Make api URI from site path', function () {
            expect(fromSitePath('', '')).to.equal(apiUri);
        });
    });

    describe('Simple item', function () {
        var sitePrefix = '/list';

        it('Api URI to site path', function () {
            expect(toSitePath('https://api.example.com/category', sitePrefix)).to.equal('/list/category');
        });

        it('Make api URI from site path', function () {
            expect(fromSitePath('/list/category', sitePrefix)).to.equal('https://api.example.com/category');
        });
    });

    describe('Nested item', function () {
        var sitePrefix = '/vendor';

        it('Api URI to site path', function () {
            expect(toSitePath('https://api.example.com/category/1cg5d', sitePrefix)).to.equal('/vendor/category/1cg5d');
        });

        it('Make api URI from site path', function () {
            expect(fromSitePath('/vendor/category/1cg5d', sitePrefix)).to.equal('https://api.example.com/category/1cg5d');
        });
    });

    describe('Collection', function () {
        var sitePrefix = '/coupons/a/';

        it('Api URI to site path', function () {

            expect(toSitePath('https://api.example.com/coupon/', sitePrefix)).to.equal('/coupons/a/coupon/');
        });

        it('Make api URI from site path', function () {
            expect(fromSitePath('/coupons/a/coupon/', sitePrefix)).to.equal('https://api.example.com/coupon/');
        });
    });

});

describe('API and client app without trailing slash', function () {
    var sitePrefix = '/coupons/a/';
    var clientUri = 'https://client.example.com:63352';
    var apiUri = 'https://api.example.com:8080';

    beforeEach(() => {
        mapping.initialise(clientUri, apiUri);
    });

    it('Api URI to site path', function () {

        expect(toSitePath('https://api.example.com:8080/coupon/', sitePrefix)).to.equal('/coupons/a/coupon/');
    });

    it('Make api URI from site path', function () {
        expect(fromSitePath('/coupons/a/coupon/', sitePrefix)).to.equal('https://api.example.com:8080/coupon/');
    });
});

describe('API and client app not in the root', function () {
    var sitePrefix = '/vendor';
    var apiUri = 'https://api.example.com/api';
    var clientUri = 'https://client.example.com/application';

    beforeEach(() => {
        mapping.initialise(clientUri, apiUri);
    });

    it('Api URI to site path', function () {
        expect(toSitePath('https://api.example.com/api/category/1cg5d', sitePrefix)).to.equal('/vendor/category/1cg5d');
    });

    it('Make api URI from site path', function () {
        expect(fromSitePath('/vendor/category/1cg5d', sitePrefix)).to.equal('https://api.example.com/api/category/1cg5d');
    });
});