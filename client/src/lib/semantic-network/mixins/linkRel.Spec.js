import * as _ from './linkRel';
import {relTypeToCamel} from './linkRel';
import {expect} from 'chai';

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

describe('rel type to camel', function () {
    it('should match string', function () {
        expect(relTypeToCamel('test')).to.equal('test');
    });
    it('should match regex', function () {
        expect(relTypeToCamel(/test/)).to.equal('test');
    });
    it('should match global regex', function () {
        expect(relTypeToCamel(/test/g)).to.equal('test');
    });
    it('should match case insensitive regex', function () {
        expect(relTypeToCamel(/test/i)).to.equal('test');
    });
    it('should match global case insensitive regex', function () {
        expect(relTypeToCamel(/test/gi)).to.equal('test');
    });
    it('should match camel case regex', function () {
        expect(relTypeToCamel(/create-form/)).to.equal('createForm');
    });
});
