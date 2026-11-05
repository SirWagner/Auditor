export default class QuestionStore {
    constructor(questionBank = []) {

        this.availableQuestions = questionBank;

        // questionId -> how many cards use it. A question can be added once per section
        // (e.g. "Are the floors clean?" on Arrival and again on Seating).
        this.usage = new Map();

        this.nextQuestionIndex = 0;
    }

    /* -----------------------------
     * QUESTION MANAGEMENT
     * ----------------------------- */

    addQuestion(questionId) {
        const id = Number(questionId);
        this.usage.set(id, (this.usage.get(id) || 0) + 1);
    }

    removeQuestion(questionId) {
        const id = Number(questionId);
        const count = (this.usage.get(id) || 0) - 1;
        if (count > 0) this.usage.set(id, count);
        else this.usage.delete(id);
    }

    hasQuestion(questionId) {
        return this.usage.has(Number(questionId));
    }

    getQuestionCount() {
        let total = 0;
        this.usage.forEach(count => { total += count; });
        return total;
    }

    /* -----------------------------
     * INDEX MANAGEMENT
     * ----------------------------- */

    getNextQuestionIndex() {
        return this.nextQuestionIndex++;
    }

    resetIndex(newIndex) {
        this.nextQuestionIndex = newIndex;
    }

    /* -----------------------------
     * QUESTION BANK ACCESS
     * ----------------------------- */

    getAvailableQuestions() {
        return this.availableQuestions;
    }

    filterQuestions(searchTerm) {
        if (!searchTerm) return this.availableQuestions;

        const normalized = searchTerm.toLowerCase();

        return this.availableQuestions.filter(q =>
            q.text.toLowerCase().includes(normalized)
        );
    }
}
