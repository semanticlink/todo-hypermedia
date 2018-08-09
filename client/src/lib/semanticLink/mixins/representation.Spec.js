import * as _ from './representation';
import {expect} from 'chai';

describe('Representation mixins', () => {

    describe('_.extendResource()', () => {

        const assertExtendResource = (resource, document) => {
            let result = _.extendResource({}, resource, document);
            expect(result).to.deep.equal(document);
        };

        it('should be able to shallow extend', () => {
            let resource = {name: 'i'};
            let document = {name: 'h'};
            assertExtendResource(resource, document);
        });

        it('should be able to deep extend', () => {
            var resource = {textBox: {width: 2, height: 6}};
            var document = {textBox: {width: 5, height: 7}};
            assertExtendResource(resource, document);
        });
        it('should be able to deep extend with mutiple at top level', () => {
            var resource = {textBox: {width: 2, height: 6}};
            var document = {name: 'h', textBox: {width: 5, height: 7}};
            assertExtendResource(resource, document);
        });

    });

    describe('_.detach()', () => {

        it('should be able to detach an array of objects', () => {
            let resource = {items: [{name: 'i'}]};
            let document = [{name: 'i'}];
            let result = _.detach(resource.items);
            expect(result).to.deep.equal(document);
        });

        it('should be able to detail a collection of items', () => {
            let resource = {items: [{name: 'i'}]};
            let document = [{name: 'i'}];
            let result = _.detach(resource);
            expect(result).to.deep.equal(document);
        });

        it('should return object on null', () => {
            expect(_.detach(null)).to.deep.equal([]);
        });

        it('should return object on undefined', () => {
            expect(_.detach(undefined)).to.deep.equal([]);
        });

    });

    describe('_.mergeByFields()', () => {

        it('should be able to shallow extend overriding matched fields', () => {
            let resource = {name: 'i'};
            let document = {name: 'h'};

            let fields = ['name'];

            let result = _.mergeByFields(resource, document, fields);
            expect(result).to.deep.equal(document);
            expect(result.name).to.equal('h');
        });

        describe('deep extend', () => {

            it('should be able to override matching field sets', () => {
                var resource = {textBox: {width: 2, height: 6}};
                var document = {textBox: {width: 5, height: 7}};

                let fields = ['name', 'textBox'];

                let result = _.mergeByFields(resource, document, fields);
                expect(result).to.deep.equal(document);
            });

            it('should be able to add to the set', () => {
                var resource = {textBox: {width: 2, height: 6}};
                var document = {name: 'copied', textBox: {width: 5, height: 7}};

                let fields = ['name', 'textBox'];

                let result = _.mergeByFields(resource, document, fields);
                expect(result).to.deep.equal(document);
            });

            it('should be able to remove from field from set', () => {
                var resource = {textBox: {width: 2, height: 6}};
                var document = {name: 'not copied', textBox: {width: 5, height: 7}};

                let fields = ['textBox'];

                let result = _.mergeByFields(resource, document, fields);
                expect(result).to.deep.equal({textBox: {width: 5, height: 7}});
            });

            it('should be able to remove from field when in both and retain value of first not in fields', () => {
                var resource = {name: 'retain', textBox: {width: 2, height: 6}};
                var document = {name: 'no override', textBox: {width: 5, height: 7}};

                let fields = ['textBox'];

                let result = _.mergeByFields(resource, document, fields);
                expect(result).to.deep.equal({name: 'retain', textBox: {width: 5, height: 7}});
            });
        });

    });

    describe('_.dashToCamel', () => {
        it('should match dashed', () => {
            expect(_.dashToCamel('question-item')).to.equal('questionItem');
        });
        it('should leave non-dashed alone', () => {
            expect(_.dashToCamel('questionitem')).to.equal('questionitem');
        });
    });

    describe('_.filterCamelToDash', () => {
        it('should match camel', () => {
            expect(_.filterCamelToDash(['questionItem'])).to.deep.equal(['question-item']);
        });
        it('should match camel and all lower', () => {
            expect(_.filterCamelToDash(['questionItem', 'question'])).to.deep.equal(['question-item']);
        });
        it('should match just all lower', () => {
            expect(_.filterCamelToDash(['question'])).to.deep.equal([]);
        });
    });

    describe('_.camelToDash', () => {

        it('should match camel on string and returns string', () => {
            expect(_.camelToDash('questionItem')).to.equal('question-item');
        });

        it('should match all lower on string and returns string', () => {
            expect(_.camelToDash('question')).to.equal('question');
        });

        it('should match camel', () => {
            expect(_.camelToDash(['questionItem'])).to.deep.equal(['question-item']);
        });

        it('should match camel and all lower', () => {
            expect(_.camelToDash(['questionItem', 'question'])).to.deep.equal(['question-item', 'question']);
        });

        it('should match just all lower', () => {
            expect(_.camelToDash(['question'])).to.deep.equal(['question']);
        });
    });

    describe('_.compactObject', () => {

        it('should do everything at once', () => {
            expect(_.compactObject({
                keep: 99,
                takeaway: {},
                alsotake: undefined,
                alsoKeep: {o: 1}
            })).to.deep.equal({keep: 99, alsoKeep: {o: 1}});
        });
        it('should keep numerical fields', () => {
            expect(_.compactObject({keep: 99})).to.deep.equal({keep: 99});
        });
        it('should keep string fields', () => {
            expect(_.compactObject({keep: 'string'})).to.deep.equal({keep: 'string'});
        });
        it('should keep object fields', () => {
            expect(_.compactObject({alsoKeep: {o: 1}})).to.deep.equal({alsoKeep: {o: 1}});
        });
        it('should remove empty object fields', () => {
            expect(_.compactObject({takeaway: {}})).to.deep.equal({});
        });

        it('should remove undefined fields', () => {
            expect(_.compactObject({takeaway: undefined})).to.deep.equal({});
        });
    });

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
});