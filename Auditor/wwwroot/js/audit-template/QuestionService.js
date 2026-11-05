export default class QuestionService {
    constructor(store) {
        this.store = store;
    }

    filterQuestions(term) {
        const normalized = term.toLowerCase();

        return this.store.availableQuestions.filter(q =>
            q.text.toLowerCase().includes(normalized)
        );
    }
}
