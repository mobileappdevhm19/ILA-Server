const {models} = require('../services/database/db');
const {body, validationResult} = require('express-validator/check');

const router = require('express').Router();

// Returns all owned courses
router.get('/', (req, res) => {
    models.Course
        .findAll({
            where: {ownerId: req.user.id},
            raw: true
        })
        .then(courses => res.json(courses))
        .catch(error => {
            console.error(error);
            res.status(500).json({errors: ['Please ty again. Internal server error.']});
        });
});

// Creates a new course
router.post('/',
    [
        body('title').not().isEmpty().withMessage('title can not be empty.'),
        body('description')
    ],
    (req, res) => {
        const errors = validationResult(req);
        if (!errors.isEmpty()) {
            return res.status(400).json({errors: errors.array()});
        }

        models.Course
            .create({
                ownerId: req.user.id,
                title: req.body.title,
                description: req.body.description
            })
            .then(course => {
                res.json(course.get().values);
            })
            .catch(error => {
                console.error(error);
                res.status(500).json({errors: ['Please ty again. Internal server error.']});
            })
    });

// Update a course
router.put('/',
    [
        body('id').isNumeric().not().isEmpty().withMessage('id can not be empty.'),
        body('title').not().isEmpty().withMessage('title can not be empty.'),
        body('description')
    ],
    (req, res) => {
        const errors = validationResult(req);
        if (!errors.isEmpty()) {
            return res.status(400).json({errors: errors.array()});
        }

        models.Course
            .findByPk(req.body.id)
            .then(course => {
                if (!course) {
                    throw {code: 404, errors: ['Couldn\'t find a course with the id.']};
                }

                if (course.ownerId === req.user.id) {
                    return course
                        .update({
                            title: req.body.title,
                            description: req.body.description
                        })
                        .then(() => res.json(course.get().values));
                } else {
                    throw {code: 403, errors: ['You can only change your own course.']};
                }
            })
            .catch(error => {
                console.error(error);
                if (!error || !error.code || error.code === 500)
                    res.status(500).json({errors: ['Please ty again. Internal server error.']});
                else
                    res.status(error.code).json({errors: error.errors});
            });
    });

// Delete a course
router.delete('/',
    [
        body('id').isNumeric().not().isEmpty().withMessage('id can not be empty.')
    ],
    (req, res) => {
        const errors = validationResult(req);
        if (!errors.isEmpty()) {
            return res.status(400).json({errors: errors.array()});
        }

        models.Course
            .findByPk(req.body.id)
            .then(course => {
                if (!course) {
                    throw {code: 404, errors: ['Couldn\'t find a course with the id.']};
                }

                if (course.ownerId === req.user.id) {
                    return course
                        .destroy()
                        .then(() => res.json({}));
                } else {
                    throw {code: 403, errors: ['You can only delete your own course.']};
                }
            })
            .catch(error => {
                console.error(error);
                if (!error || !error.code || error.code === 500)
                    res.status(500).json({errors: ['Please ty again. Internal server error.']});
                else
                    res.status(error.code).json({errors: error.errors});
            });
    });

module.exports = router;