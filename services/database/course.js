module.exports = (sequelize, DataTypes) => {
    const Course = sequelize.define('courses', {
        id: {
            type: DataTypes.INTEGER,
            primaryKey: true,
            autoIncrement: true
        },
        title: {
            type: DataTypes.STRING,
            allowNull: false
        },
        description: DataTypes.STRING,
        // owner
    }, {
        timestamps: false
    });

    Course.associate = models => {
        Course.belongsTo(models.User, {foreignKey: 'ownerId', as: 'owner'});
        Course.belongsToMany(models.User, {as: 'Members', through: 'course_members', foreignKey: 'courseId',
            timestamps: false});
        Course.hasMany(models.CourseToken, {onDelete: 'CASCADE', foreignKey: 'courseId'});
        Course.hasMany(models.Lecture, {onDelete: 'CASCADE', foreignKey: 'courseId'});
    };

    return Course;
};

