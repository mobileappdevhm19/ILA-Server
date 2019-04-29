module.exports = (sequelize, DataTypes) => {
    const User = sequelize.define('users', {
        id: {
            type: DataTypes.INTEGER,
            primaryKey: true,
            autoIncrement: true
        },
        name: DataTypes.STRING,
        password: DataTypes.STRING,
        email: DataTypes.STRING,
        emailApproved: DataTypes.BOOLEAN
    }, {
        timestamps: false
    });

    User.associate = models => {
        User.hasMany(models.Course, { onDelete: 'CASCADE', as: ' owner', foreignKey: 'ownerId' });
    };

    return User;
};