export default class QuestionStore {
    constructor(questionBank = []) {
        this.availableQuestions = questionBank;

        this.addedQuestionIds = new Set();

        // This becomes the ONLY index source of truth
        this.nextQuestionIndex = 0;
    }

    /* -----------------------------
     * QUESTION MANAGEMENT
     * ----------------------------- */

    addQuestion(questionId) {
        this.addedQuestionIds.add(questionId);
    }

    removeQuestion(questionId) {
        this.addedQuestionIds.delete(questionId);
    }

    hasQuestion(questionId) {
        return this.addedQuestionIds.has(questionId);
    }

    getAddedQuestionIds() {
        return this.addedQuestionIds;
    }

    getQuestionCount() {
        return this.addedQuestionIds.size;
    }

    /* -----------------------------
     * INDEX MANAGEMENT (IMPORTANT FIX)
     * ----------------------------- */

    /**
     * IMPORTANT:
     * This guarantees sequential index generation
     * and removes manual increment calls everywhere.
     */
    getNextQuestionIndex() {
        return this.nextQuestionIndex++;
    }

    /**
     * Used ONLY after deletion/rebuild scenarios
     */
    resetIndex(newIndex) {
        this.nextQuestionIndex = newIndex;
    }

    /**
     * Rebuild index based on current UI state (safe sync)
     */
    syncIndexToCount(currentCount) {
        this.nextQuestionIndex = currentCount;
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